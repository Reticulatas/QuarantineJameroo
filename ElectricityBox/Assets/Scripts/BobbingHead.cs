using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class BobbingHead : MonoBehaviour
{
    [SerializeField] private float startY;
    [SerializeField] private float upDownSpeed;
    [SerializeField] private float upDownAmplitude;
    
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;
        float currentPos = (float)Math.Sin(upDownSpeed * Time.time * Math.PI * 2.0f) * upDownAmplitude;
        transform.position = new Vector3(
            pos.x,
            startY + currentPos,
                pos.z
            );
        
    }
}
