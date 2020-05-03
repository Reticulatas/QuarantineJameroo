using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseManager : MonoBehaviour
{
    public int SceneToDumpLoserTo;

    void Start()
    {
        Time.timeScale = 0.1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1.0f;
            SceneManager.LoadScene(SceneToDumpLoserTo, LoadSceneMode.Single);
        }
    }
}
