using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IWantsBeats
{
    void OnBeat();
    void OnBigBeat();
}

public class GameManager : BehaviourSingleton<GameManager>
{
    private float gameTime;
    private ulong beats;

    public const int BEATSINABIGBEAT = 8;
    public const float BIGBEATTIME = 1.0f;
    public const float BEATTIMER = BIGBEATTIME / BEATSINABIGBEAT;

    public int LeftSideSceneIndex;
    public int RightSideSceneIndex;
    public GameObject[] DestroyAfterLoading;

    public int Schmunny = 0;

    private bool lost = false;
    public bool Lost => lost;

    private bool shopping;

    private readonly List<IWantsBeats> beatWanters = new List<IWantsBeats>();

    public event Action<int> DamageDelt;

    public void Start()
    {
        SceneManager.LoadScene(LeftSideSceneIndex, LoadSceneMode.Additive);
        SceneManager.LoadScene(RightSideSceneIndex, LoadSceneMode.Additive);
        foreach (var o in DestroyAfterLoading)
            Destroy(o);
    }

    public void Register(IWantsBeats beater)
    {
        beatWanters.Add(beater);
    }
    public void UnRegister(IWantsBeats beater)
    {
        beatWanters.Remove(beater);
    }

    public void Lose()
    {
        lost = true;
        SceneManager.LoadSceneAsync("Lose", LoadSceneMode.Additive);
    }

    public void AddMoney(int amount)
    {
        Schmunny += amount;
        Debug.Log("Schmunny: " + Schmunny);
    }

    public void DealDamage(int amount)
    {
        DamageDelt?.Invoke(amount);
    }

    public void ShowShop()
    {
        shopping = true;
        SceneManager.LoadScene("shop", LoadSceneMode.Additive);
    }

    public void HideShop()
    {
        shopping = false;
        SceneManager.UnloadSceneAsync("shop");
    }

    public void Update()
    {
        if (lost || shopping)
            return;

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Lose();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ShowShop();
        }
        #endif

        gameTime += Time.deltaTime;

        if (gameTime > BEATTIMER)
        {
            gameTime = gameTime - BEATTIMER;
            ++beats;

            if (beats % BEATSINABIGBEAT == 0)
            {
                // dispatch da big beats
                foreach (var beater in beatWanters.ToList())
                {
                    beater.OnBigBeat();
                }
            }
            else
            {
                // dispatch da beats
                foreach (var beater in beatWanters.ToList())
                {
                    beater.OnBeat();
                }
            }
        }
    }
}
