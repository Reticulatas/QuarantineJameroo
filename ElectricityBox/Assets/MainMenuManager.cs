using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Animator Animator;
    public int MainSceneIndex;
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

    void Update()
    {
        if (starting)
        {
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("DONE"))
            {
                SceneManager.LoadScene(MainSceneIndex);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Animator.SetTrigger("Start");
            starting = true;
        }

        AudioListener.volume = volumeSlider.value;
    }
}
