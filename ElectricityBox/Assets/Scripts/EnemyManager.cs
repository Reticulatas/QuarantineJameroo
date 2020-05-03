using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour, IWantsBeats
{
    public int NextEnemyHealth;
    
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private float timeToEnter;
    [SerializeField] private int bigBeatsToSpawnEnemy;
    
    private int bigBeatCounter = 0;

    public event Action<GameObject> EnemySpawned;
    public event Action EnemyDestroyed;
    
    private Enemy CurrentEnemy;
    
    
    private class Enemy
    {
        public Enemy(GameObject _obj, TMP_Text _healthText, int _maxHealth)
        {
            obj = _obj;
            healthText = _healthText;
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
        public int health;
        public float displayedHealth;
        public readonly TMP_Text healthText;
        public int maxHealth;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GameManager.obj.Register(this);
        GameManager.obj.DamageDelt += ObjOnDamageDelt;
        
    }


    void Awake()
    {
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
    }

    void UpdateHealthText()
    {
        if (CurrentEnemy == null) return;

        float displayedHealth = CurrentEnemy.displayedHealth;
        int health = CurrentEnemy.health;
        displayedHealth = (Mathf.MoveTowards(displayedHealth, (float)health + 0.5f, 1.0f));
        CurrentEnemy.SetDisplayedHealth(displayedHealth);
        CurrentEnemy.healthText.SetText($"{Mathf.Floor(displayedHealth).ToString()}");
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
        newEnemy.GetComponent<EnemyScript>().TargetPosition = spawnPosition;
        newEnemy.GetComponent<EnemyScript>().MoveTime = timeToEnter;

        CurrentEnemy = new Enemy(newEnemy, newEnemy.GetComponentInChildren<TMP_Text>(), health);
        
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
