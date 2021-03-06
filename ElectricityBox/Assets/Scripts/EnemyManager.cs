﻿using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : BehaviourSingleton<EnemyManager>, IWantsBeats
{
    public int NextEnemyHealth;

    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] public float TimeToEnter;
    [SerializeField] private int bigBeatsToSpawnEnemy;

    [SerializeField] private float rotSpeedMin;
    [SerializeField] private float rotSpeedMax;

    [SerializeField] private float noiseResolutionMin;
    [SerializeField] private float noiseResolutionMax;
    [SerializeField] private float noiseSpeedMin;
    [SerializeField] private float noiseSpeedMax;

    [SerializeField] private float bobAmplitude;
    [SerializeField] private float bobSpeed;

    [SerializeField] private Vector3 textOffset;

    [SerializeField] private Transform baseLauncher;
    [SerializeField] private Transform topLauncher;
    [SerializeField] private Transform frontLauncher;
    [SerializeField] private Transform backLauncher;

    [SerializeField] private GameObject missileObject;
    [SerializeField] private float missileTravelTime;
    [SerializeField] private float missileOutwardRatio;
    [SerializeField] private float missileOutwardDistance;

    private int bigBeatCounter = 0;

    public event Action<GameObject> EnemySpawned;
    public event Action EnemyDestroyed;

    private Enemy CurrentEnemy;
    private TMP_Text healthText;
    private RectTransform healthTextTransform;

    public GameObject KoreanMMOTextPrefab;

    public AudioSource AudioSource;
    public AudioClip SFX_MissileShot;
    public AudioClip SFX_MissileHit;
    public AudioClip SFX_EnemyDeath;

    private class Enemy
    {
        public Enemy(GameObject _obj, int _maxHealth)
        {
            obj = _obj;
            transform = _obj.transform;
            health = _maxHealth;
            displayedHealth = (float) _maxHealth;
            maxHealth = _maxHealth;
        }

        public void SetHealth(int _health)
        {
            health = Mathf.Clamp(_health, 0, 999);
        }

        public void SetDisplayedHealth(float _health)
        {
            displayedHealth = _health;
        }

        public readonly GameObject obj;
        public readonly Transform transform;
        public int health;
        public float displayedHealth;
        public int maxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.obj.Register(this);
        GameManager.obj.DamageDelt += ObjOnDamageDelt;
        healthText = GetComponentInChildren<TMP_Text>();
        healthTextTransform = GetComponentInChildren<RectTransform>();
    }


    public override void Awake()
    {
        base.Awake();
    }


    // Update is called once per frame
    void Update()
    {
        UpdateHealthText();

        if (Input.GetKeyDown(KeyCode.F7))
            DestroyEnemy();
    }

    void OnDestroy()
    {
        GameManager.obj.UnRegister(this);
        GameManager.obj.DamageDelt -= ObjOnDamageDelt;
    }

    void DestroyEnemy()
    {
        Destroy(CurrentEnemy.obj);
        CurrentEnemy = null;
        EnemyDestroyed?.Invoke();
        GameManager.obj.OnEnemyKilled();
        PackGridManager.obj.RemoveAll(GridObject.Type.JUNK);
    }

    void UpdateHealthText()
    {
        if (CurrentEnemy == null)
        {
            healthText.SetText("");
            return;
        }

        float displayedHealth = CurrentEnemy.displayedHealth;
        int health = CurrentEnemy.health;
        displayedHealth = (Mathf.MoveTowards(displayedHealth, (float) health + 0.5f, 1.0f));
        CurrentEnemy.SetDisplayedHealth(displayedHealth);

        healthText.SetText($"{Mathf.Floor(displayedHealth).ToString()}");
        healthTextTransform.anchoredPosition3D = CurrentEnemy.transform.position + textOffset;
    }

    private void ObjOnDamageDelt(int damage)
    {
        if (CurrentEnemy == null) return;

        LaunchMissile(damage);
        StartCoroutine(Co_DealDamage(damage));
    }

    private void LaunchMissile(int damage)
    {
        switch (damage)
        {
            case 1:
                CreateMissile(topLauncher, false);
                break;
            case 2:
                CreateMissile(topLauncher, true);
                break;
            case 3:
                CreateMissile(topLauncher, true);
                CreateMissile(frontLauncher, false);
                break;
            case 4:
                CreateMissile(topLauncher, true);
                CreateMissile(frontLauncher, true);
                break;
            case 5:
                CreateMissile(topLauncher, true);
                CreateMissile(frontLauncher, true);
                CreateMissile(backLauncher, false);
                break;
            default:
                CreateMissile(topLauncher, true);
                CreateMissile(frontLauncher, true);
                CreateMissile(backLauncher, true);
                break;
        }
    }

    private void CreateMissile(Transform launcher, bool beeg)
    {
        var missileObj = Instantiate(missileObject);
        var missile = missileObj.GetComponent<Missile>();
        missile.EnemyTransform = CurrentEnemy.transform;
        missile.OutwardTravelTime = missileOutwardRatio * missileTravelTime;
        missile.OutwardVector =
            Vector3.Normalize(launcher.position - baseLauncher.position) * missileOutwardDistance;
        missile.TotalTravelTime = missileTravelTime;
        missile.LauncherTransform = launcher;
        missile.Enemy = CurrentEnemy.obj;

        if (beeg == false)
        {
            missile.transform.localScale *= 0.5f;
        }

        if (SFX_MissileShot != null)
            AudioSource.PlayOneShot(SFX_MissileShot);
    }

    private IEnumerator Co_DealDamage(int damage)
    {
        var waitForSeconds = new WaitForSeconds(missileTravelTime);
        yield return waitForSeconds;

        if (CurrentEnemy == null) yield break;

        // calc combo
        int actualDamage = damage < 10 ? damage * 2 : 999;
        CurrentEnemy.SetHealth(CurrentEnemy.health - actualDamage);

        if (SFX_MissileHit != null)
            AudioSource.PlayOneShot(SFX_MissileHit);

        Instantiate(KoreanMMOTextPrefab, CurrentEnemy.obj.transform.position, Quaternion.identity).GetComponent<KoreanMMOText>().Init(actualDamage);

        if (CurrentEnemy.health <= 0)
        {
            if (SFX_EnemyDeath != null)
                AudioSource.PlayOneShot(SFX_EnemyDeath);

            var rend = CurrentEnemy.obj.GetComponentInChildren<MeshRenderer>();
            yield return DOVirtual.Float(1.1f, 0.0f, 2.0f, (value =>
            {
                rend.material.SetFloat("_Fade", value);
            })).WaitForCompletion();

            if (NextEnemyHealth % 2 == 0)
                NextEnemyHealth += 1;
            else
                NextEnemyHealth += 2;
            GameManager.obj.SpeedUp();
            DestroyEnemy();
        }
    }


    void GenerateEnemy(int health = 100)
    {
        int i = Random.Range(0, enemies.Length);

        var newEnemy = Instantiate(enemies[i], gameObject.transform);
        var enemyScript = newEnemy.GetComponent<EnemyScript>();
        enemyScript.TargetPosition = spawnPosition;
        enemyScript.MoveTime = TimeToEnter;
        enemyScript.RotationSpeed = Random.Range(rotSpeedMin, rotSpeedMax);
        enemyScript.BobAmplitude = bobAmplitude;
        enemyScript.BobSpeed = bobSpeed;

        var material = newEnemy.GetComponentInChildren<MeshRenderer>().material;
        material.SetFloat("_Resolution", Random.Range(noiseResolutionMin, noiseResolutionMax));
        material.SetFloat("_Speed", Random.Range(noiseSpeedMin, noiseSpeedMax));

        CurrentEnemy = new Enemy(newEnemy, health);

        EnemySpawned.Invoke(newEnemy);
    }

    public void OnBeat()
    {

    }

    public void OnBigBeat()
    {
        if (CurrentEnemy != null) return;

        ++bigBeatCounter;
        if (bigBeatCounter >= bigBeatsToSpawnEnemy)
        {
            GenerateEnemy(NextEnemyHealth);
            bigBeatCounter = 0;
        }
    }
}

