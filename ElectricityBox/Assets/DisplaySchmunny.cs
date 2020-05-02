using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        text.SetText($"${Mathf.Floor(storedSchmunny).ToString()}");
    }
}
