using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectGenerator : MonoBehaviour
{
    [SerializeField] 
    private GameObject[] objects = null;

    [SerializeField] private float spawnInterval = 0.1f;

    [SerializeField] private Vector3 spawnMinPos = Vector3.zero;
    [SerializeField] private Vector3 spawnMaxPos = Vector3.zero;
    [SerializeField] private int spawnMinRotationSpeed = 0;
    [SerializeField] private int spawnMaxRotationSpeed = 0;
    [SerializeField] private int spawnMinRotation = 0;
    [SerializeField] private int spawnMaxRotation = 0;

    [SerializeField] private float spawnMinMoveSpeed = 0.0f;
    [SerializeField] private float spawnMaxMoveSpeed = 0.0f;
    [SerializeField] private float spawnMinSize = 1.0f;
    [SerializeField] private float spawnMaxSize = 1.0f;

    private struct Environment
    {
        public Environment(Transform _transform, int _rotationSpeed, float _moveSpeed)
        {
            transform = _transform;
            rotationSpeed = _rotationSpeed;
            moveSpeed = _moveSpeed;
        }
        
        public Transform transform;
        public int rotationSpeed;
        public float moveSpeed;
    }

    private readonly List<Environment?> managedObjects = new List<Environment?>();

    private float objectsToSpawn = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 1200; ++i)
        {
            Spawn(Time.fixedDeltaTime);
            Advance(Time.fixedDeltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Spawn(Time.deltaTime);
        Advance(Time.deltaTime);
    }

    void Spawn(float dt)
    {
        int random = Random.Range(0, objects.Length);

        objectsToSpawn += dt / spawnInterval;

        for (; objectsToSpawn > 1.0f; --objectsToSpawn)
        {
            GameObject newObj = Instantiate(objects[random], gameObject.transform);

            var pos = new Vector3(
                Random.Range(spawnMinPos.x, spawnMaxPos.x),
                Random.Range(spawnMinPos.y, spawnMaxPos.y),
                Random.Range(spawnMinPos.z, spawnMaxPos.z)
            );

            var rot = Random.Range(spawnMinRotation, spawnMaxRotation);
            newObj.transform.SetPositionAndRotation(pos, Quaternion.AngleAxis(rot, Vector3.up));
            newObj.transform.localScale *= Random.Range(spawnMinSize, spawnMaxSize);

            var rotSpeed = Random.Range(spawnMinRotationSpeed, spawnMaxRotationSpeed);
            var moveSpeed = Random.Range(spawnMinMoveSpeed, spawnMaxMoveSpeed);
            var env = new Environment(newObj.transform, rotSpeed, moveSpeed);

            bool isFull = true;
            for (var i = 0; i < managedObjects.Count; i++)
            {
                if (!managedObjects[i].HasValue)
                {
                    managedObjects[i] = env;
                    isFull = false;
                    break;
                }
            }

            if (isFull)
                managedObjects.Add(env);
        }
    }

    void Advance(float dt)
    {
        for (var index = 0; index < managedObjects.Count; index++)
        {
            var env = managedObjects[index];
            if (!env.HasValue) continue;

            var pos = env.Value.transform.position;
            pos.x -= env.Value.moveSpeed * dt;

            env.Value.transform.Rotate(Vector3.up, env.Value.rotationSpeed * dt);
            env.Value.transform.position = pos;

            if (pos.x < -400.0f)
            {
                Destroy(env.Value.transform.gameObject);
                managedObjects[index] = null;
            }
        }
    }
}
