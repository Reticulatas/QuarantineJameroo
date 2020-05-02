using System.Collections.Generic;
using DG.Tweening;
using MoreLinq;
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
        CIG,

        MAX
    }
    public Type ObjType;
    public GameObject[] EnableIfType;

    private readonly List<GridObject> boundTo = new List<GridObject>();
    public IEnumerable<GridObject> BoundedTo => boundTo;

    public int X;
    public int Y;

    public GameObject LinePrefab;
    private LineRenderer[] Lines;

    GridObject()
    {
        ID = ++IDCounter;
    }

    public void MakeBoundTo(GridObject obj, bool makeLines = true)
    {
        if (boundTo.Contains(obj))
            return;
        boundTo.Add(obj);
        obj.MakeBoundTo(this);
        if (makeLines)
            FormatLines();
    }
    public void DestroyBindsTo(GridObject obj, bool makeLines = true)
    {
        if (!boundTo.Contains(obj))
            return;
        boundTo.Remove(obj);
        obj.DestroyBindsTo(this);
        if (makeLines)
            FormatLines();
    }

    private void FormatLines()
    {
        // remove
        Lines?.ForEach(x => Destroy(x.gameObject));

        // make new
        Lines = new LineRenderer[boundTo.Count];
        for (var index = 0; index < boundTo.Count; index++)
        {
            var gridObject = boundTo[index];
            var line = Instantiate(LinePrefab, transform);
            Lines[index] = line.GetComponent<LineRenderer>();
        }
    }
    private void PositionLines()
    {
        // make new
        for (var index = 0; index < boundTo.Count; index++)
        {
            var gridObject = boundTo[index];
            var line = Lines[index];

            var dir = (gridObject.transform.position - transform.position).normalized;
            var lposOrigin = dir * 0.25f;

            line.SetPosition(0, lposOrigin);
            line.SetPosition(1, gridObject.transform.position - transform.position - lposOrigin);
        }
    }

    public void DestroyAllBinds()
    {
        var gridObjects = boundTo.ToListPooled();
        foreach (var gridObject in gridObjects)
        {
            DestroyBindsTo(gridObject, false);
        }
        gridObjects.Free();
        FormatLines();
    }


    public void Update()
    {
        PositionLines();
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
            moveTween = transform.DOLocalMove(intendedWorldPos, GameManager.BEATTIMER * .9f).SetEase(Ease.InCubic);
        }
    }

    public void OnBeat()
    {
        if (!IsDying)
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
        
        moveTween?.Kill();

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

        moveTween?.Kill();

        GameManager.obj.UnRegister(this);

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(new Vector3(0.9f, 0.9f, 0.9f), 0.1666f));
        seq.AppendInterval(GameManager.BEATTIMER + 0.333f);
        seq.AppendCallback(() => { Destroy(gameObject); });
        seq.Play();
    }
}