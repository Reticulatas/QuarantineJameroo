using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    public float RangeLower, RangeHigher;
    public float CycleTime = 3.0f;
    public bool DontScaleZ = false;
    public bool IgnoreTimescale = false;

    private float timer = 0;
	
	void Update ()
	{
	    timer += IgnoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
	    if (timer > CycleTime)
	        timer = 0;
	    float t = Mathf.Clamp01(timer / CycleTime);
	    float c = RangeLower + ((Mathf.Sin(t*Mathf.PI*2) + 1) / 2.0f) * (RangeHigher - RangeLower);
        transform.localScale = new Vector3(c,c, DontScaleZ ? transform.localScale.z : c);
	}
}
