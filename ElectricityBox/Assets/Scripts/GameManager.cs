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
    private uint beats;

    public uint BEATSINABIGBEAT = 7;
    public float BIGBEATTIME = 0.8f;
    public float BEATTIMER
    {
        get { return BIGBEATTIME / BEATSINABIGBEAT; }
    }

    public int LeftSideSceneIndex;
    public int RightSideSceneIndex;
    public GameObject[] DestroyAfterLoading;

    public int Schmunny = 0;

    public int paybacks = 0;
    public TMPro.TextMeshProUGUI PaybacksText;

    private bool lost = false;
    public bool Lost => lost;

    private bool shopping;

    private readonly List<IWantsBeats> beatWanters = new List<IWantsBeats>();

    public event Action<int> DamageDelt;

    [Flags]
    public enum Upgrade
    {
        NONE = 0,
        MOAR = 1,
        EXPLOSIVE = 1 << 1,
        STATIC = 1 << 2
    }
    public Upgrade UnlockedUpgrades { get; private set; }

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

    private int upgrades = 0;
    public void AddMoney(int amount)
    {
        Schmunny = Mathf.Clamp(Schmunny + amount, 0, 99999);
        Debug.Log("Schmunny: " + Schmunny);

        var nextUpgradeMoney = GetMoneyForNextUpgrade();
        if (Schmunny >= nextUpgradeMoney)
        {
            Schmunny -= nextUpgradeMoney;
            ShowShop();
        }
    }

    public void SpeedUpMajor()
    {
        BIGBEATTIME *= 0.6f;
        ++upgrades;
    }
    public void SpeedUp()
    {
        BIGBEATTIME *= 0.8f;
    }

    public int GetMoneyForNextUpgrade()
    {
        const int MoneyForUpgrade = 25;
        switch (upgrades)
        {
            case 0:
                return 50;
            case 1:
                return 150;
            case 2:
                return 500;
            default:
                return 999;
        }
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

    public void UnlockUpgrade(Upgrade up)
    {
        UnlockedUpgrades |= up;
        ++upgrades;
    }

    public void OnEnemyKilled()
    {
        SpeedUp();
        ++paybacks;
        PaybacksText.text = $"<size=200%><color=orange>{paybacks}</color></size> paybacks";
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
            gameTime = 0;
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
