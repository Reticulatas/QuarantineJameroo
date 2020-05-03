using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float time = 1.0f;
    private float t = 0;

    public bool UnscaledTime = true;

    private bool destroyed = false;
	
	void Update ()
    {
        if (!destroyed)
        {
            t += UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (t >= time)
            {
                Destroy(gameObject);
                destroyed = true;
            }
        }
    }
}
