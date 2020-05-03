using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

public class TutorialManager : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        [SerializeField]
        public GameObject obj;
        [SerializeField]
        public int step;
    }
    public List<Step> steps;
    public int MainSceneIndex;

    public GameObject PressEnter;

    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        StartCoroutine(Co_Tutorial());
    }

    public IEnumerator Co_Tutorial()
    {
        int step = 0;
        var waitForSeconds = new WaitForSeconds(0.2f);

        while (steps.Any(x => !x.obj.activeSelf))
        {
            PressEnter.SetActive(false);
            do
            {
                var step1 = step;
                steps.Where(x => x.step <= step1).Select(x => x.obj).ForEach(x => x.SetActive(true));
                this.GetComponent<AudioSource>().Play();

                yield return waitForSeconds;
                ++step;
            } while (step % 5 != 0);

            if (steps.All(x => x.obj.activeSelf))
            {
                PressEnter.GetComponent<TMPro.TextMeshProUGUI>().text = "<color=orange>SPACE</color> to <color=#05ffa1>BEGIN</color>";
            }

            PressEnter.SetActive(true);
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }
        }

        this.GetComponent<AudioSource>().Play();
        SceneManager.LoadScene(MainSceneIndex, LoadSceneMode.Single);
    }
}
