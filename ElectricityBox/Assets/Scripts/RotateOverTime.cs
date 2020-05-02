using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public int XSpeed, YSpeed, ZSpeed;
    public bool IgnoreTimeScale = true;

    void Update()
    {
        float time = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        transform.Rotate(XSpeed * time, YSpeed * time, ZSpeed * time);
    }
}