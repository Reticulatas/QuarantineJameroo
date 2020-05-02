using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;
using Util;
using Random = UnityEngine.Random;

public class PackGridManager : MonoBehaviour , IWantsBeats
{
    public const int GRIDW = 4;
    public const int GRIDH = 10;
    public const int LOADERH = 7;
    public const int MINTODESTROYJUNK = 3;

    public GameObject BlockPrefab;
    public Transform BlockSpawnLocation;

    private int blockSpawnTimer = 0;
    private TileMap.Dir? queuedDirMove = null; 

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

                foreach (var bound in gridObject.BoundTo)
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

        public enum Dir { NORTH, SOUTH, EAST, WEST }
        private void TransformPointDir(Dir dir, int x, int y, out int nx, out int ny)
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

            var toCheck = tile.BoundTo.ToListPooled();
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

        public bool RemoveObjectAt(int x, int y)
        {
            var obj = ObjectAt(x, y);
            if (obj == null)
                return false;

            obj.DestroyAllBinds();
            objects.Remove(obj);
            obj.X = -1;
            obj.Y = -1;
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

    private int spawned = 0;
    private const int EVERYXISAMMO = 5;
    public void SpawnNewGridObject()
    {
        var poll = new SimplePoll<pair<int, int>>();

        for (int x = 0; x < GRIDW; ++x)
        {
            for (int y = 0; y < GRIDH/2; ++y)
            {
                if (!map.SomethingAt(x, y) && map.ObjectAt(x, y) == null)
                    poll.Vote(new pair<int, int>(x, y));
            }
        }

        // no possible
        if (poll.Votes.Count == 0)
            return;

        var point = poll.Result;

        ++spawned;

        var newBlock = Instantiate(BlockPrefab, transform);
        newBlock.transform.localPosition = BlockSpawnLocation.localPosition;
        var gridObject = newBlock.GetComponent<GridObject>();

        // assign type
        GridObject.Type type = (GridObject.Type) Random.Range(0, (int) GridObject.Type.MAX);
        if (spawned % EVERYXISAMMO == 0)
            type = GridObject.Type.AMMO;
        gridObject.ObjType = type;

        map.SetObjectAt(gridObject, point.First, point.Second);
        gridObject.StartMoveToIntended();
    }

    private void MapBeat()
    {
        // remove those needing to be removed
        var blocks = map.IterateAllBlocks().ToListPooled();
        foreach (var block in blocks)
        {
            if (block == null || block.IsDying)
                continue;
            
            if (block.ObjType == GridObject.Type.AMMO)
            {
                if (block.Y >= LOADERH)
                {
                    map.RemoveObjectAt(block.X, block.Y);
                    block.BeginFiring();
                }
            }
            else if (block.ObjType == GridObject.Type.JUNK)
            {
                List<GridObject> objsToRemove = ListPool<GridObject>.New();
                map.FloodCollect(block.X, block.Y, ref objsToRemove, obj => obj.ObjType == GridObject.Type.JUNK);
                if (objsToRemove.Count >= MINTODESTROYJUNK)
                {
                    foreach (var gridObject in objsToRemove)
                    {
                        if (map.RemoveObjectAt(gridObject.X, gridObject.Y))
                            gridObject.BeginConsume();
                    }
                }

            }
        }
        blocks.Free();

        // slide
        if (queuedDirMove.HasValue)
        {
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
            SpawnNewGridObject();
        }
    }
}
