using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public List<Button> UpgradeButtons;

    public void Start()
    {
        for (var i = 0; i < UpgradeButtons.Count; i++)
        {
            UpgradeButtons[i].interactable = !GameManager.obj.UnlockedUpgrades.HasFlag((GameManager.Upgrade)(1<<i));
        }
    }

    public void OnUpgrade(int which)
    {
        GameManager.obj.UnlockUpgrade((GameManager.Upgrade)(1 << which));
        GameManager.obj.HideShop();
    }
    public void OnCancel()
    {
        GameManager.obj.SpeedUp();
        GameManager.obj.HideShop();
    }
}
