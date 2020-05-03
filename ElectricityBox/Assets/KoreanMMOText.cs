using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class KoreanMMOText : MonoBehaviour
{
    public TMPro.TextMeshPro text;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            Init(5);
    }

    public void Init(int dmg)
    {
        text.text = "0";

        var seq = DOTween.Sequence();
        seq.Append(text.transform.DOLocalMove(new Vector3(Random.Range(-3, 3), 3, Random.Range(-3, 3)), 0.33f).SetEase(Ease.OutCubic));
        seq.Join(text.transform.DOPunchScale(Vector3.one * .05f * dmg, 0.9f, 2, 0.2f).SetEase(Ease.OutBounce));
        seq.Join(DOVirtual.Float(0, (float) dmg + 0.5f, 0.466f, value => text.text = $"{Mathf.FloorToInt(value)}"));
        seq.Append(text.DOFade(0, 3.0f));
        seq.AppendCallback(() =>
        {
            Destroy(gameObject);
        });
        seq.Play();

    }
}
