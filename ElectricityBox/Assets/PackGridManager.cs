﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Analytics;
using Util;
using Random = UnityEngine.Random;

public class PackGridManager : BehaviourSingleton<PackGridManager>, IWantsBeats
{
    public const int GRIDW = 4;
    public const int GRIDH = 10;
    public const int LOADERH = 7;
    public const int MINTODESTROYJUNK = 3;

    public GameObject BlockPrefab;
    public Transform BlockSpawnLocation;
    public Transform Root;
    public GameObject RopesTutorialText;

    private bool firstRope = true;

    private int blockSpawnTimer = 0;
    private TileMap.Dir? queuedDirMove = null;

    public Animator ShutterAnimator;

    private bool shouldClearLoaded = false;

    public AudioClip SFX_LoadedAmmo;
    public AudioClip SFX_CigBlocksCleared;
    public AudioClip SFX_AllStaticCleared;
    public AudioClip SFX_NewBlock;
    public AudioClip SFX_Nudge;
    public AudioSource AudioSource;

    private class TileMap
    {
        private readonly TileType[,] tiles = new TileType[GRIDW, GRIDH];
        private readonly List<GridObject> objects = new List<GridObject>();

        public TileMap()
        {
            for (var x = 0; x < tiles.GetLength(0); x++)
            {
                for (var y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y] = TileType.EMPTY;
                }
            }

            for (int i = GRIDH - 3; i < GRIDH; ++i)
            {
                tiles[0, i] = TileType.BLOCKED;
                tiles[GRIDW - 1, i] = TileType.BLOCKED;
            }
        }

        // iterates tiles, bottom up
        public IEnumerable<GridObject> IterateAllBlocks()
        {
            return objects;
        }

        public IEnumerable<GridObject> IterateUniqueBlocks()
        {
            HashSet<GridObject> objs = HashSetPool<GridObject>.New();
            foreach (var gridObject in objects)
            {
                if (objs.Contains(gridObject))
                    continue;

                yield return gridObject;

                foreach (var bound in gridObject.BoundedTo)
                {
                    objs.Add(bound);
                }
                objs.Add(gridObject);
            }
            objs.Free();
        }

        public void DebugLog()
        {
            StringBuilder ss = new StringBuilder();
            for (var y = 0; y < tiles.GetLength(1); y++)
            {
                for (var x = 0; x < tiles.GetLength(0); x++)
                {
                    ss.Append($"{tiles[x, y]}");
                }

                ss.AppendLine();
            }
            Debug.Log(ss.ToString());
        }

        public GridObject ObjectAt(int x, int y)
        {
            return objects.Find(t => t.X == x && t.Y == y);
        }

        public enum Dir { NORTH, SOUTH, EAST, WEST, MAX }
        public void TransformPointDir(Dir dir, int x, int y, out int nx, out int ny)
        {
            int rx = x;
            int ry = y;
            switch (dir)
            {
                case Dir.NORTH:
                    ry -= 1;
                    break;
                case Dir.SOUTH:
                    ry += 1;
                    break;
                case Dir.EAST:
                    rx += 1;
                    break;
                case Dir.WEST:
                    rx -= 1;
                    break;
            }
            nx = rx;
            ny = ry;
        }

        public bool SlideTile(GridObject tile, Dir dir)
        {
            // find one over point this tile can go

            var toCheck = tile.BoundedTo.ToListPooled();
            toCheck.Add(tile);

            bool canMove = true;
            foreach (var gridObject in toCheck)
            {
                int nx, ny;
                TransformPointDir(dir, gridObject.X, gridObject.Y, out nx, out ny);

                if (OutOfBounds(nx, ny))
                {
                    // oob
                    canMove = false;
                    break;
                }

                if (SomethingAt(nx, ny))
                {
                    // we've hit a blocker
                    canMove = false;
                    break;
                }

                if (gridObject.ObjType != GridObject.Type.AMMO)
                {
                    // junk can't go in the loader
                    if (ny >= LOADERH)
                    {
                        canMove = false;
                        break;
                    }
                }

                var obj = ObjectAt(nx, ny);
                if (obj != null && !toCheck.Contains(obj))
                {
                    // we're not self colliding and this is empty, we're no good 
                    canMove = false;
                    break;
                }
            }

            if (canMove)
            {
                // perform the move
                foreach (var gridObject in toCheck)
                {
                    int nx, ny;
                    TransformPointDir(dir, gridObject.X, gridObject.Y, out nx, out ny);
                    gridObject.X = nx;
                    gridObject.Y = ny;
                }
            }

            toCheck.Free();

            return canMove;
        }

        public bool SomethingAt(int x, int y)
        {
            return tiles[x, y] != TileType.EMPTY;
        }

        public void SetTypeAt(TileType type, int x, int y)
        {
            tiles[x, y] = type;
        }

        public bool SetObjectAt(GridObject obj, int x, int y)
        {
            if (SomethingAt(x, y))
            {
                Debug.LogError($"Setting an object at {x},{y} while there's something there!");
                return false;
            }

            obj.X = x;
            obj.Y = y;
            objects.Add(obj);
            return true;
        }

        public bool CanPlaceAt(int x, int y)
        {
            if (OutOfBounds(x, y))
                return false;
            if (SomethingAt(x, y))
                return false;
            if (ObjectAt(x, y) != null)
                return false;
            return true;
        }

        public bool RemoveObjectAt(int x, int y)
        {
            var obj = ObjectAt(x, y);
            if (obj == null)
                return false;

            objects.Remove(obj);
            obj.X = -1;
            obj.Y = -1;

            if (GameManager.obj.UnlockedUpgrades.HasFlag(GameManager.Upgrade.EXPLOSIVE))
            {
                var boundObjs = obj.BoundedTo.Where(r => r.ObjType == GridObject.Type.JUNK).ToListPooled();
                foreach (var gridObject in boundObjs)
                {
                    if (gridObject != null)
                    {
                        if (RemoveObjectAt(gridObject.X, gridObject.Y))
                            gridObject.BeginConsume();
                    }
                }
                boundObjs.Free();
            }

            if (GameManager.obj.UnlockedUpgrades.HasFlag(GameManager.Upgrade.STATIC))
            {
                if (obj.ObjType == GridObject.Type.CIG)
                {
                    void RemoveNearbyJunk(int nx, int ny)
                    {
                        var gridObject = ObjectAt(nx, ny);
                        if (!OutOfBounds(nx, ny) && gridObject?.ObjType == GridObject.Type.JUNK)
                        {
                            if (RemoveObjectAt(nx, ny))
                                gridObject.BeginConsume();
                        }
                    }
                    RemoveNearbyJunk(x + 1, y);
                    RemoveNearbyJunk(x - 1, y);
                    RemoveNearbyJunk(x, y + 1);
                    RemoveNearbyJunk(x, y - 1);
                }
            }

            obj.DestroyAllBinds();
            return true;
        }

        public bool OutOfBounds(int x, int y)
        {
            return (x < 0 || x >= GRIDW || y < 0 || y >= GRIDH);
        }

        public void FloodCollect(int _x, int _y, ref List<GridObject> ret, Func<GridObject,bool> comparator)
        {
            void InnerFunc(int x, int y, ref List<GridObject> found, HashSet<pair<int, int>> searched)
            {
                var current = new pair<int, int>(x, y);
                if (searched.Contains(current))
                    return;
                searched.Add(current);

                var obj = ObjectAt(x, y);
                if (obj == null)
                    return;

                if (comparator == null || comparator.Invoke(obj))
                {
                    found.Add(obj);

                    InnerFunc(x + 1, y, ref found, searched);
                    InnerFunc(x - 1, y, ref found, searched);
                    InnerFunc(x, y + 1, ref found, searched);
                    InnerFunc(x, y - 1, ref found, searched);
                }
            }

            var search = HashSetPool<pair<int, int>>.New();
            InnerFunc(_x,_y, ref ret, search);
            search.Free();
        }
    }
    private readonly TileMap map = new TileMap();

    public enum TileType
    {
        EMPTY,
        BLOCKED,
    }

    public void RemoveAll(GridObject.Type type)
    {
        if (SFX_AllStaticCleared != null)
            AudioSource.PlayOneShot(SFX_AllStaticCleared);

        foreach (var block in map.IterateAllBlocks().ToList())
        {
            if (block.ObjType == type)
            {
                map.RemoveObjectAt(block.X, block.Y);
                block.BeginConsume();
            }
        }
    }


    private int spawned = 0;
    private const int EVERYXISJUNK = 7;
    private const int EVERYXISAMMO = 4;
    public bool SpawnNewGridObject()
    {
        var poll = new WeightedPoll<pair<int, int>>();

        for (int x = 0; x < GRIDW; ++x)
        {
            for (int y = 0; y < LOADERH ; ++y)
            {
                if (map.CanPlaceAt(x, y))
                    poll.Vote(new pair<int, int>(x, y));
            }
        }

        // no possible
        if (poll.Votes.Count == 0)
            return false;

        GridObject MakeObjectAt(int x, int y)
        {
            ++spawned;

            var newBlock = Instantiate(BlockPrefab, transform);
            newBlock.transform.localPosition = BlockSpawnLocation.localPosition;
            var gridObject = newBlock.GetComponent<GridObject>();

            // assign type
            var typePoll = new WeightedPoll<GridObject.Type>();
            typePoll.Vote(GridObject.Type.AMMO, spawned % EVERYXISAMMO == 0 ? 100 : 2);
            typePoll.Vote(GridObject.Type.JUNK, spawned % EVERYXISJUNK == 0 ? 50 : GameManager.obj.paybacks);
            typePoll.Vote(GridObject.Type.CIG, 8);
            gridObject.ObjType = typePoll.WeightedRandomResult;

            map.SetObjectAt(gridObject, x, y);
            gridObject.StartMoveToIntended(Ease.OutQuart);

            return gridObject;
        }

        var point = poll.WeightedRandomResult;
        var block1 = MakeObjectAt(point.First, point.Second);

        if (block1.ObjType == GridObject.Type.CIG)
        {
            if (GameManager.obj.paybacks > 1)
            {
                // make a second one if we can
                TileMap.Dir dir = (TileMap.Dir) Random.Range(0, (int) TileMap.Dir.MAX);
                int nx, ny;
                map.TransformPointDir(dir, point.First, point.Second, out nx, out ny);
                if (map.CanPlaceAt(nx, ny) && ny < LOADERH)
                {
                    var block2 = MakeObjectAt(nx, ny);
                    block2.MakeBoundTo(block1);

                    if (firstRope)
                    {
                        RopesTutorialText.SetActive(true);
                        firstRope = false;
                    }
                }
            }
        }

        if (SFX_NewBlock != null)
            AudioSource.PlayOneShot(SFX_NewBlock);

        return true;
    }

    private void MapBeat()
    {
        int clearedDuringLoad = 0;
        if (shouldClearLoaded)
        {
            if (SFX_LoadedAmmo != null)
                AudioSource.PlayOneShot(SFX_LoadedAmmo);
            ShutterAnimator.SetTrigger("Open");
        }

        // remove those needing to be removed
        var blocks = map.IterateAllBlocks().ToListPooled();
        foreach (var block in blocks)
        {
            if (block == null || block.IsDying)
                continue;
            
            if (block.ObjType == GridObject.Type.AMMO)
            {
                if (shouldClearLoaded)
                {
                    if (block.Y >= LOADERH)
                    {
                        map.RemoveObjectAt(block.X, block.Y);
                        block.BeginFiring();
                        ++clearedDuringLoad;
                    }
                }
            }
            else if (/*block.ObjType == GridObject.Type.JUNK ||*/ block.ObjType == GridObject.Type.CIG)
            {
                List<GridObject> objsToRemove = ListPool<GridObject>.New();
                map.FloodCollect(block.X, block.Y, ref objsToRemove, obj => obj.ObjType == block.ObjType);
                if (objsToRemove.Count >= MINTODESTROYJUNK)
                {
                    if (block.ObjType == GridObject.Type.CIG)
                    {
                        GameManager.obj.AddMoney(objsToRemove.Count * 5);
                    }

                    foreach (var gridObject in objsToRemove)
                    {
                        if (map.RemoveObjectAt(gridObject.X, gridObject.Y))
                            gridObject.BeginConsume();
                    }

                    if (SFX_CigBlocksCleared != null)
                        AudioSource.PlayOneShot(SFX_CigBlocksCleared);
                }

            }
        }
        blocks.Free();

        // slide
        if (queuedDirMove.HasValue)
        {
            float punch = 5;
            Root.DOComplete();
            switch (queuedDirMove.Value)
            {
                case TileMap.Dir.NORTH:
                    Root.DOPunchRotation(new Vector3(punch*3, 0, 0), 0.166f);
                    break;
                case TileMap.Dir.SOUTH:
                    Root.DOPunchRotation(new Vector3(-punch*3, 0, 0), 0.166f);
                    break;
                case TileMap.Dir.EAST:
                    Root.DOPunchRotation(new Vector3(0, 0, -punch), 0.166f);
                    break;
                case TileMap.Dir.WEST:
                    Root.DOPunchRotation(new Vector3(0, 0, punch), 0.166f);
                    break;
            }

            if (SFX_Nudge != null)
                AudioSource.PlayOneShot(SFX_Nudge);

            bool somethingSlid = true;
            do
            {
                bool anythingSlid = false;
                foreach (var unqBlock in map.IterateUniqueBlocks())
                {
                    anythingSlid |= map.SlideTile(unqBlock, queuedDirMove.Value);
                }
                somethingSlid = anythingSlid;
            } while (somethingSlid);

            queuedDirMove = null;
        }

        if (clearedDuringLoad > 0)
        {
            if (clearedDuringLoad >= 6 && GameManager.obj.UnlockedUpgrades.HasFlag(GameManager.Upgrade.MOAR))
                GameManager.obj.DealDamage(999);
            else
                GameManager.obj.DealDamage(clearedDuringLoad);
        }

        shouldClearLoaded = false;
    }

    void Start()
    {
        GameManager.obj.Register(this);
    }

    void OnDestroy()
    {
        GameManager.obj.UnRegister(this);
    }

    void Update()
    {
        if (GameManager.obj.Lost)
            return;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            map.DebugLog();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            queuedDirMove = TileMap.Dir.NORTH;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            queuedDirMove = TileMap.Dir.EAST;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            queuedDirMove = TileMap.Dir.WEST;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            queuedDirMove = TileMap.Dir.SOUTH;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            shouldClearLoaded = true;
        }
    }

    public void OnBeat()
    {
        MapBeat();
    }

    public void OnBigBeat()
    {
        ++blockSpawnTimer;
        if (blockSpawnTimer % 2 == 0)
        {
            if (!SpawnNewGridObject())
            {
                GameManager.obj.Lose();
            }
        }
    }
}
