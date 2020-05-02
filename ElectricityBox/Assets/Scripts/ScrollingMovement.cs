using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingMovement : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 10.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;

        pos.x -= Time.deltaTime * moveSpeed;
        transform.position = new Vector3(pos.x, pos.y, pos.z);
        
        if (pos.x < -400.0f)
            Destroy(gameObject);
        
    }
}
