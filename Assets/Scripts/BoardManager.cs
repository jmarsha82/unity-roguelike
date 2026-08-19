using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
    public WallObject WallPrefab;
    public FoodObject FoodPrefab;
    public ExitCellObject ExitCellPrefab;
    public CellObject EnemyPrefab;
    public int BaseWallCount = 6;
    public int BaseFoodCount = 5;
    public int BaseEnemyCount = 1;

    public void Init(int level = 1)
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        //Initialize the list
        m_EmptyCellsList = new List<Vector2Int>();

        m_BoardData = new CellData[Width, Height];


        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;

                    //this is a passable empty cell, add it to the list!
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        if (ExitCellPrefab != null)
        {
            AddObject(Instantiate(ExitCellPrefab), endCoord);
        }

        m_EmptyCellsList.Remove(endCoord);
        GenerateWall(level);
        GenerateFood(level);
        GenerateEnemies(level);
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width
            || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    void GenerateWall(int level)
    {
        if (WallPrefab == null)
        {
            return;
        }

        int wallCount = Mathf.Min(BaseWallCount + level, m_EmptyCellsList.Count);
        for (int i = 0; i < wallCount; ++i)
        {
            WallObject newWall = Instantiate(WallPrefab);
            AddObjectAtRandomEmptyCell(newWall);
        }
    }

    void GenerateFood(int level)
    {
        if (FoodPrefab == null)
        {
            return;
        }

        int foodCount = Mathf.Min(BaseFoodCount + Mathf.FloorToInt(level * 0.5f), m_EmptyCellsList.Count);
        for (int i = 0; i < foodCount; ++i)
        {
            FoodObject newFood = Instantiate(FoodPrefab);
            AddObjectAtRandomEmptyCell(newFood);
        }
    }

    void GenerateEnemies(int level)
    {
        if (EnemyPrefab == null)
        {
            return;
        }

        int enemyCount = Mathf.Min(BaseEnemyCount + Mathf.FloorToInt(level * 0.5f), m_EmptyCellsList.Count);
        for (int i = 0; i < enemyCount; ++i)
        {
            CellObject newEnemy = Instantiate(EnemyPrefab);
            AddObjectAtRandomEmptyCell(newEnemy);
        }
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    bool AddObjectAtRandomEmptyCell(CellObject obj)
    {
        if (m_EmptyCellsList.Count == 0)
        {
            Destroy(obj.gameObject);
            return false;
        }

        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        Vector2Int coord = m_EmptyCellsList[randomIndex];

        m_EmptyCellsList.RemoveAt(randomIndex);
        AddObject(obj, coord);
        return true;
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void ClearCellObject(Vector2Int cellIndex, CellObject obj)
    {
        CellData data = GetCellData(cellIndex);
        if (data != null && data.ContainedObject == obj)
        {
            data.ContainedObject = null;
        }
    }

    public void Clean()
    {
        //no board data, so exit early, nothing to clean
        if (m_BoardData == null)
            return;


        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    //CAREFUL! Destroy the GameObject NOT just cellData.ContainedObject
                    //Otherwise what you are destroying is the JUST CellObject COMPONENT
                    //and not the whole gameobject with sprite
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }
}
