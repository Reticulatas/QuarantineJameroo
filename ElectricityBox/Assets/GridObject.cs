using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridObject : MonoBehaviour, IWantsBeats
{
    public const int INVALIDID = -1;
    private static int IDCounter = 0;
    public int ID { get; }
    public bool IsDying { get; private set; }

    public enum Type
    {
        JUNK,
        AMMO,

        MAX
    }
    public Type ObjType;
    public GameObject[] EnableIfType;

    public HashSet<GridObject> BoundTo = new HashSet<GridObject>();

    public int X;
    public int Y;

    GridObject()
    {
        ID = ++IDCounter;
    }

    public void MakeBoundTo(GridObject obj)
    {
        obj.BoundTo.Add(this);
        BoundTo.Add(obj);
    }
    public void DestroyBindsTo(GridObject obj)
    {
        obj.BoundTo.Remove(this);
        BoundTo.Remove(obj);
    }

    public void DestroyAllBinds()
    {
        var gridObjects = BoundTo.ToListPooled();
        foreach (var gridObject in gridObjects)
        {
            DestroyBindsTo(gridObject);
        }
        gridObjects.Free();
    }


    public void Awake()
    {
        GameManager.obj.Register(this);
    }

    public void Start()
    {
        for (var i = 0; i < EnableIfType.Length; i++)
            EnableIfType[i].SetActive((int)ObjType == i);
    }

    public void OnDestroy()
    {
        GameManager.obj.UnRegister(this);
    }

    private Vector3 GetIntendedWorldPos()
    {
        return new Vector3(X, -Y, 0);
    }

    private Tween moveTween;

    public void StartMoveToIntended()
    {
        if (moveTween != null && moveTween.IsPlaying())
        {
            return;
        }
        
        var intendedWorldPos = GetIntendedWorldPos();
        if (Vector3.Distance(transform.position, intendedWorldPos) > float.Epsilon)
        {
            moveTween = transform.DOLocalMove(intendedWorldPos, GameManager.BEATTIMER * 0.5f).SetEase(Ease.InBack);
        }
    }

    public void OnBeat()
    {
        StartMoveToIntended();
    }
    public void OnBigBeat()
    {
    }

    public void BeginConsume()
    {
        if (IsDying)
            return;
        IsDying = true;

        GameManager.obj.UnRegister(this);

        var seq = DOTween.Sequence();
        seq.Append(transform.DOPunchScale(Vector3.one * 1.5f, 0.1666f, 2, 0.3f));
        seq.AppendInterval(0.1f);
        foreach (var rend in gameObject.GetComponentsInChildren<SpriteRenderer>())
            seq.Join(rend.DOFade(0, 0.2f));
        seq.AppendCallback(() => { Destroy(gameObject); });
        seq.Play();
    }
    public void BeginFiring()
    {
        if (IsDying)
            return;
        IsDying = true;

        GameManager.obj.UnRegister(this);

        var seq = DOTween.Sequence();
        seq.Append(transform.DOPunchScale(Vector3.one * 0.5f, 0.333f, 2, 0.1f));
        seq.Append(transform.DOLocalMoveY(-20, 0.5f).SetEase(Ease.InBack));
        seq.AppendCallback(() => { Destroy(gameObject); });
        seq.Play();
    }
}