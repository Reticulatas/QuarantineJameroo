using System.Collections;
using System.Collections.Generic;
using System.Timers;
using DG.Tweening;
using SCPSim.Util;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public Vector3 OutwardVector;
    public Vector3 StartPosition;
    public Transform LauncherTransform;
    public Transform EnemyTransform;
    public float TotalTravelTime;
    public float OutwardTravelTime;

    private float timer = 0.0f;
    

    enum Phase
    {
        OUT = 0,
        TOWARD
    }

    private Phase phase;
    private Vector3 midPosition;
    private float towardTravelTime;

    // Start is called before the first frame update
    void Start()
    {
        phase = Phase.OUT;
        StartPosition = transform.position = LauncherTransform.position;
        transform.up = Vector3.Normalize(OutwardVector);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < OutwardTravelTime)
        {
            transform.position = StartPosition + OutwardVector * DOVirtual.EasedValue(0.0f, 1.0f, timer / OutwardTravelTime, Ease.OutCubic);
        }
        else if (timer < TotalTravelTime)
        {
            var position = transform.position;
            if (phase == Phase.OUT)
            {
                phase = Phase.TOWARD;
                StartPosition = position;
                midPosition = (EnemyTransform.position + StartPosition) / 2.0f;
                midPosition += OutwardVector;
            }

            float percentage = (timer - OutwardTravelTime) / (TotalTravelTime - OutwardTravelTime);
            var newPosition = Curve.CubicBezier(
                                     DOVirtual.EasedValue(0.0f, 1.0f, percentage, Ease.InQuart),
                                     StartPosition,
                                     midPosition,
                                     midPosition,
                                     EnemyTransform.position);

            
            transform.up = Vector3.Lerp(transform.up, Vector3.Normalize(newPosition - position), 0.5f);
            transform.position = newPosition;
        }
        else
        {
            Destroy(gameObject);
        }

        timer += Time.deltaTime;
    }
}
