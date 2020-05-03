using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceSetToMainCamera : MonoBehaviour
{
    public Canvas canvas;


    void Update()
    {
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 1;
    }
}
