using System;
using System.Collections;
using System.Collections.Generic;
using Data.Util;
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
            health = _health;
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

        if (Input.GetKeyDown(KeyCode.Alpha1))
            ObjOnDamageDelt(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ObjOnDamageDelt(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            ObjOnDamageDelt(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            ObjOnDamageDelt(4);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            ObjOnDamageDelt(5);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            ObjOnDamageDelt(6);
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
        EnemyDestroyed.Invoke();
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
            case 6:
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
    }

    private IEnumerator Co_DealDamage(int damage)
    {
        var waitForSeconds = new WaitForSeconds(missileTravelTime);
        yield return waitForSeconds;

        if (CurrentEnemy == null) yield break;

        CurrentEnemy.SetHealth(CurrentEnemy.health - damage);

        if (CurrentEnemy.health <= 0)
        {
            NextEnemyHealth = Mathf.FloorToInt(NextEnemyHealth * 1.75f);
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

