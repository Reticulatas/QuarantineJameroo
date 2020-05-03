using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public List<Button> UpgradeButtons;

    private bool somethingPicked = false;

    public void Start()
    {
        for (var i = 0; i < UpgradeButtons.Count; i++)
        {
            UpgradeButtons[i].interactable = !GameManager.obj.UnlockedUpgrades.HasFlag((GameManager.Upgrade)(1<<i));
        }
    }

    public void Update()
    {
        if (somethingPicked)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            OnUpgrade(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            OnUpgrade(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            OnUpgrade(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            OnCancel();
    }

    public void OnUpgrade(int which)
    {
        somethingPicked = true;
        GameManager.obj.UnlockUpgrade((GameManager.Upgrade)(1 << which));
        GameManager.obj.HideShop();
    }
    public void OnCancel()
    {
        somethingPicked = true;
        GameManager.obj.SpeedUpMajor();
        GameManager.obj.HideShop();
    }
}
