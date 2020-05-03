using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnemyScript : MonoBehaviour
{
    public Vector3 TargetPosition;
    public float MoveTime;
    public float RotationSpeed;

    public float BobSpeed;
    public float BobAmplitude;

    private float moveTimer = 0.0f;

    private Vector3 velocity;
    private Vector3 startPosition;

    private Vector3 rotationAxis;
    
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        velocity = (TargetPosition - startPosition) / MoveTime;

        int i = UnityEngine.Random.Range(0, 3);

        switch (i)
        {
            case 0:
                rotationAxis = Vector3.up;
                break;
            case 1:
                rotationAxis = Vector3.right;
                break;
            case 2:
                rotationAxis = Vector3.back;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ApproachTarget();
    }

    private void ApproachTarget()
    {
        Vector3 bob = new Vector3(0.0f, Mathf.Sin(moveTimer * 3.14159f * 2.0f * BobSpeed) * BobAmplitude);
        transform.position = startPosition + velocity * Mathf.Min(moveTimer, MoveTime) + bob;

        moveTimer += Time.deltaTime;
        
        transform.Rotate(rotationAxis, RotationSpeed * Time.deltaTime);
    }

}
