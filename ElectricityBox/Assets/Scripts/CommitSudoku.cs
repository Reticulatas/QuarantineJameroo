using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommitSudoku : MonoBehaviour
{
    [SerializeField] public float SudokuTime;


    private float timer = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > SudokuTime)
            Destroy(gameObject);

        timer += Time.deltaTime;
    }
}
