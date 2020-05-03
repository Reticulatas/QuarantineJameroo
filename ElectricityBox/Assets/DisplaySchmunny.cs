using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;

public class DisplaySchmunny : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private float storedSchmunny = 0.0f;
    private TMP_Text text;
    
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        storedSchmunny = Mathf.MoveTowards(storedSchmunny, (float)gameManager.Schmunny + 0.5f, 1.0f);
        int nextUpgrade = gameManager.GetMoneyForNextUpgrade() - Mathf.FloorToInt(storedSchmunny);

        text.SetText($"${Mathf.FloorToInt(storedSchmunny)}{Environment.NewLine}<size=45%><smallcaps>upgrade in ${nextUpgrade}</smallcaps></size>");
    }
}
