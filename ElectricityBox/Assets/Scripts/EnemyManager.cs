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
            displayedHealth = (float)_maxHealth;
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

        if (Input.GetKeyDown(KeyCode.F3))
        {
            DestroyEnemy();
        }
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
        displayedHealth = (Mathf.MoveTowards(displayedHealth, (float)health + 0.5f, 1.0f));
        CurrentEnemy.SetDisplayedHealth(displayedHealth);
        
        healthText.SetText($"{Mathf.Floor(displayedHealth).ToString()}");
        healthTextTransform.anchoredPosition3D = CurrentEnemy.transform.position + textOffset;
    }
    
    private void ObjOnDamageDelt(int damage)
    {
        if (CurrentEnemy == null) return;

        CurrentEnemy.SetHealth(CurrentEnemy.health - damage);
        if (CurrentEnemy.health <= 0)
        {
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
