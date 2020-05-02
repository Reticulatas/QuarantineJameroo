using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectGenerator : MonoBehaviour
{
    [SerializeField] 
    private GameObject[] objects;

    [SerializeField] private float spawnInterval = 0.1f;

    [SerializeField] private Vector3 spawnMinPos;
    [SerializeField] private Vector3 spawnMaxPos;
    [SerializeField] private int spawnMinRotationSpeed;
    [SerializeField] private int spawnMaxRotationSpeed;
    [SerializeField] private float spawnMinMoveSpeed;
    [SerializeField] private float spawnMaxMoveSpeed;
    [SerializeField] private float spawnMinSize = 1.0f;
    [SerializeField] private float spawnMaxSize = 1.0f;
    

    private float objectsToSpawn = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int random = Random.Range(0, objects.Length);

        objectsToSpawn += Time.deltaTime / spawnInterval;
        
        for (; objectsToSpawn > 1.0f; --objectsToSpawn)
        {
            GameObject newObj = Instantiate(objects[random]);
            var pos = new Vector3(
                Random.Range(spawnMinPos.x, spawnMaxPos.x),
                Random.Range(spawnMinPos.y, spawnMaxPos.y),
                Random.Range(spawnMinPos.z, spawnMaxPos.z)
            );

            var rot = Random.Range(spawnMinRotationSpeed, spawnMaxRotationSpeed);
            newObj.transform.SetPositionAndRotation(pos, Quaternion.AngleAxis(rot, Vector3.up));
            newObj.transform.localScale *= Random.Range(spawnMinSize, spawnMaxSize);
            var rotation = newObj.AddComponent<RotateOverTime>();
            rotation.IgnoreTimeScale = false;
            rotation.YSpeed = Random.Range(spawnMinRotationSpeed, spawnMaxRotationSpeed);
        
            var scrolling = newObj.AddComponent<ScrollingMovement>();
            scrolling.moveSpeed = Random.Range(spawnMinMoveSpeed, spawnMaxMoveSpeed);
        }
    }
}
