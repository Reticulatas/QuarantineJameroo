using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public void OnUpgrade(int which)
    {
        GameManager.obj.HideShop();
    }
    public void OnCancel()
    {
        GameManager.obj.HideShop();
    }
}
