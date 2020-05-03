using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Animator Animator;
    public int MainSceneIndex;
    public int TutorialSceneIndex;
    public GameObject SettingsMenu;

    public Slider volumeSlider;

    private bool starting = false;

    void Start()
    {
        volumeSlider.value = 1.0f;
        AudioListener.volume = 1.0f;
    }

    public void ToggleSettingsMenu()
    {
        SettingsMenu.SetActive(!SettingsMenu.activeSelf);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    private static bool hasVisitedTutorialScene = false;
    void Update()
    {
        if (starting)
        {
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("DONE"))
            {
                if (!hasVisitedTutorialScene)
                    SceneManager.LoadScene(TutorialSceneIndex);
                else
                    SceneManager.LoadScene(MainSceneIndex);
                hasVisitedTutorialScene = true;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Animator.SetTrigger("Start");
            this.GetComponent<AudioSource>().Play();
            starting = true;
        }

        AudioListener.volume = volumeSlider.value;
    }
}
