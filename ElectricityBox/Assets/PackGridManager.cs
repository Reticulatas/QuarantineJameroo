using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public const int INVALIDID = -1;
    private static int IDCounter = 0;
    public int ID { get; }

    public int IntendedX { get; set; }
    public int IntendedY { get; set; }

    GridObject()
    {
        ID = ++IDCounter;
    }

    public void Update()
    {

    }
}

public class PackGridManager : MonoBehaviour 
{
    public const int GRIDW = 3;
    public const int GRIDH = 10;

    private class TileMap
    {
        private readonly Tile[,] tiles = new Tile[GRIDW, GRIDH];

        public TileMap()
        {
            for (var x = 0; x < tiles.GetLength(0); x++)
            {
                for (var y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x,y] = new Tile();
                    tiles[x,y].SetEmpty();
                }
            }
        }

        public IEnumerable<Tile> IterateTiles()
        {
            for (var x = 0; x < tiles.GetLength(0); x++)
            {
                for (var y = 0; y < tiles.GetLength(1); y++)
                {
                    yield return tiles[x, y];
                }
            }
        }

        public void DebugLog()
        {
            StringBuilder ss = new StringBuilder();
            for (var y = 0; y < tiles.GetLength(1); y++)
            {
                for (var x = 0; x < tiles.GetLength(0); x++)
                {
                    ss.Append($"{tiles[x, y].Type}");
                }

                ss.AppendLine();
            }
            Debug.Log(ss.ToString());
        }

        public bool SomethingAt(int x, int y)
        {
            return tiles[x, y].Type != TileType.EMPTY;
        }

        public void SetObjectAt(int x, int y, int id)
        {
            if (SomethingAt(x, y))
                Debug.LogError($"Setting an object at {x},{y} while there's something there!");
            tiles[x,y].SetGridObject(id);
        }
    }
    private readonly TileMap map = new TileMap();

    public enum TileType
    {
        EMPTY,
        BLOCKED,
        GRIDOBJECT,
    }
                
    public class Tile
    {
        public TileType Type;
        public int ID;

        public void SetBlocked()
        {
            Type = TileType.BLOCKED;
            ID = GridObject.INVALIDID;
        }
        public void SetEmpty()
        {
            Type = TileType.EMPTY;
            ID = GridObject.INVALIDID;
        }
        public void SetGridObject(int id)
        {
            Type = TileType.GRIDOBJECT;
            ID = id;
        }
    }

    public void AddObjectToGrid(GridObject obj, int x, int y)
    {
        map.SetObjectAt(x, y, obj.ID);
    }

    void Start()
    {
        foreach (var iterateTile in map.IterateTiles())
        {
            iterateTile.SetBlocked();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            map.DebugLog();
        }
    }
}
