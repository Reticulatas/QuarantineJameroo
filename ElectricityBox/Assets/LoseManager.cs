using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseManager : MonoBehaviour
{
    public int SceneToDumpLoserTo;
    public Image Mask;

    private bool ending;

    void Start()
    {
        Time.timeScale = 0.1f;
    }

    void Update()
    {
        if (ending)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ending = true;
            Time.timeScale = 1.0f;

            Mask.DOFillAmount(1.0f, 2.5f).onComplete += () =>
            {
                SceneManager.LoadScene(SceneToDumpLoserTo, LoadSceneMode.Single);
            };
        }
    }
}
