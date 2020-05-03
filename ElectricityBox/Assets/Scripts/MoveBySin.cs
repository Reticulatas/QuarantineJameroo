using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBySin : MonoBehaviour
{
    [Header("Note this takes the original local position as the origin")]
    public float XAmplitude = 0.0f;
    public float YAmplitude = 0.0f;
    public float ZAmplitude = 1.0f;
    public float XSpeed = 1.0f, YSpeed = 1.0f, ZSpeed = 1.0f;

    private Vector3 originalPosition;
    private Vector3 offsetPos = Vector3.zero;
    private float time = 0; 

    void Start()
    {
        originalPosition = transform.localPosition;
    }
	
	void Update ()
	{
	    time += Time.smoothDeltaTime;

	    offsetPos.x = Mathf.Sin(time * XSpeed) * XAmplitude;
	    offsetPos.y = Mathf.Sin(time * YSpeed) * YAmplitude;
	    offsetPos.z = Mathf.Sin(time * ZSpeed) * ZAmplitude;

	    transform.localPosition = originalPosition + offsetPos;
	}
}
