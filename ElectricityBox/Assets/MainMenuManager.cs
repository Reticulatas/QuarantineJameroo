using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public Animator Animator;

    private bool starting = false;

    void Update()
    {
        if (starting)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Animator.SetTrigger("Start");
            starting = true;
        }
    }
}
