using System.Collections;
using System.Collections.Generic;
using System.Timers;
using DG.Tweening;
using SCPSim.Util;
using UnityEngine;
using UnityEngine.VFX;

public class Missile : MonoBehaviour
{
    public GameObject Explosion;
    
    public Vector3 OutwardVector;
    public Vector3 StartPosition;
    public Transform LauncherTransform;
    public Transform EnemyTransform;
    public float TotalTravelTime;
    public float OutwardTravelTime;
    public GameObject Enemy;

    private float timer = 0.0f;
    

    enum Phase
    {
        OUT = 0,
        TOWARD
    }

    private Phase phase;
    private Vector3 midPosition;
    private float towardTravelTime;
    private VisualEffect vfx;

    // Start is called before the first frame update
    void Start()
    {
        phase = Phase.OUT;
        StartPosition = transform.position = LauncherTransform.position;
        transform.up = Vector3.Normalize(OutwardVector);
        vfx = GetComponentInChildren<VisualEffect>();
        vfx.SetVector3("DownVector", -transform.up);
    }

    // Update is called once per frame
    void Update()
    {
        if (Enemy == null)
        {
            DestroySelf();
            return;
        }
        
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
            if (percentage > 0.7f) 
                GetComponentInChildren<VisualEffect>().Stop();
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
            DestroySelf();
        }

        timer += Time.deltaTime;
    }


    private void DestroySelf()
    {
        var explosion = Instantiate(Explosion);
        var evfx = explosion.GetComponent<VisualEffect>();
            
        // evfx.SetVector3("Position", transform.position);
        evfx.transform.position = transform.position;
            
        Destroy(gameObject);
    }
}
