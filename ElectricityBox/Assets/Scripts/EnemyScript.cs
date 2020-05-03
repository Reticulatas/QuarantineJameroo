using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public Vector3 TargetPosition;
    public float MoveTime;

    private float moveTimer = 0.0f;

    private Vector3 velocity;
    private Vector3 startPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        velocity = (TargetPosition - startPosition) / MoveTime;
    }

    // Update is called once per frame
    void Update()
    {
        ApproachTarget();
    }

    private void ApproachTarget()
    {
        transform.position = startPosition + velocity * Mathf.Min(moveTimer, MoveTime);

        moveTimer += Time.deltaTime;
    }

}
