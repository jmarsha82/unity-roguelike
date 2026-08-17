using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class BoardManagerTests : InputTestFixture
{
    [Test]
    public void BoardManagerGeneratesOneTileWallBorderAndGroundInterior()
    {
        GameObject boardObject = new GameObject("BoardManager");
        boardObject.AddComponent<Grid>();

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(boardObject.transform);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();

        Tile groundTile = ScriptableObject.CreateInstance<Tile>();
        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        Component boardManager = boardObject.AddComponent(FindBoardManagerType());

        SetField(boardManager, "Width", 4);
        SetField(boardManager, "Height", 4);
        SetField(boardManager, "GroundTiles", new[] { groundTile });
        SetField(boardManager, "WallTiles", new[] { wallTile });

        try
        {
            Invoke(boardManager, "Init");

            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 4; ++x)
                {
                    TileBase expected = x == 0 || y == 0 || x == 3 || y == 3 ? wallTile : groundTile;
                    Assert.AreSame(expected, tilemap.GetTile(new Vector3Int(x, y, 0)), $"Unexpected tile at ({x}, {y}).");
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(groundTile);
            UnityEngine.Object.DestroyImmediate(wallTile);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void BoardManagerReportsPassableInteriorAndNullOutOfBounds()
    {
        GameObject boardObject = new GameObject("BoardManager");
        boardObject.AddComponent<Grid>();

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(boardObject.transform);
        tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();

        Tile groundTile = ScriptableObject.CreateInstance<Tile>();
        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        Component boardManager = boardObject.AddComponent(FindBoardManagerType());

        SetField(boardManager, "Width", 3);
        SetField(boardManager, "Height", 3);
        SetField(boardManager, "GroundTiles", new[] { groundTile });
        SetField(boardManager, "WallTiles", new[] { wallTile });

        try
        {
            Invoke(boardManager, "Init");

            Assert.That(GetField(Invoke(boardManager, "GetCellData", new Vector2Int(1, 1)), "Passable"), Is.True);
            Assert.That(GetField(Invoke(boardManager, "GetCellData", new Vector2Int(0, 1)), "Passable"), Is.False);
            Assert.That(Invoke(boardManager, "GetCellData", new Vector2Int(-1, 1)), Is.Null);
            Assert.That(Invoke(boardManager, "GetCellData", new Vector2Int(3, 1)), Is.Null);
            Assert.That(Invoke(boardManager, "GetCellData", new Vector2Int(1, -1)), Is.Null);
            Assert.That(Invoke(boardManager, "GetCellData", new Vector2Int(1, 3)), Is.Null);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(groundTile);
            UnityEngine.Object.DestroyImmediate(wallTile);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerMovesUpIntoPassableCell()
    {
        GameObject gameManagerObject = CreateGameManager(out Component gameManager, out object turnManager);
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));

            object moved = Invoke(playerController, "TryMove", Vector2Int.up);

            Assert.That(moved, Is.True);
            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 2)));
            Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(2));
        }
        finally
        {
            ResetGameManagerInstance();
            UnityEngine.Object.DestroyImmediate(gameManagerObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerMovesRightIntoPassableCell()
    {
        GameObject gameManagerObject = CreateGameManager(out Component gameManager, out object turnManager);
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));
            Vector3 startingPosition = playerObject.transform.position;

            object moved = Invoke(playerController, "TryMove", Vector2Int.right);

            Assert.That(moved, Is.True);
            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(playerObject.transform.position, Is.Not.EqualTo(startingPosition));
            Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(2));

            Vector3 movedPosition = playerObject.transform.position;
            object blocked = Invoke(playerController, "TryMove", Vector2Int.right);

            Assert.That(blocked, Is.False);
            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(playerObject.transform.position, Is.EqualTo(movedPosition));
            Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(2));
        }
        finally
        {
            ResetGameManagerInstance();
            UnityEngine.Object.DestroyImmediate(gameManagerObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerBlocksDownIntoImpassableCell()
    {
        GameObject gameManagerObject = CreateGameManager(out Component gameManager, out object turnManager);
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));
            Vector3 startingPosition = playerObject.transform.position;

            object moved = Invoke(playerController, "TryMove", Vector2Int.down);

            Assert.That(moved, Is.False);
            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(playerObject.transform.position, Is.EqualTo(startingPosition));
            Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(1));
        }
        finally
        {
            ResetGameManagerInstance();
            UnityEngine.Object.DestroyImmediate(gameManagerObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerBlocksLeftIntoImpassableCell()
    {
        GameObject gameManagerObject = CreateGameManager(out Component gameManager, out object turnManager);
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));
            Vector3 startingPosition = playerObject.transform.position;

            object moved = Invoke(playerController, "TryMove", Vector2Int.left);

            Assert.That(moved, Is.False);
            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(playerObject.transform.position, Is.EqualTo(startingPosition));
            Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(1));
        }
        finally
        {
            ResetGameManagerInstance();
            UnityEngine.Object.DestroyImmediate(gameManagerObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerIgnoresFramesWithoutInput()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        GameObject gameManagerObject = CreateGameManager(out Component gameManager, out object turnManager);
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));
            Vector3 startingPosition = playerObject.transform.position;

            Invoke(playerController, "Update");

            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(playerObject.transform.position, Is.EqualTo(startingPosition));
            Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(1));
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            ResetGameManagerInstance();
            UnityEngine.Object.DestroyImmediate(gameManagerObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerUpdateUsesArrowKeysForMovement()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();

        AssertArrowKeyMovesPlayer(keyboard.upArrowKey, new Vector2Int(1, 2), 2);
        AssertArrowKeyMovesPlayer(keyboard.rightArrowKey, new Vector2Int(2, 1), 2);
        AssertArrowKeyMovesPlayer(keyboard.downArrowKey, new Vector2Int(1, 1), 1);
        AssertArrowKeyMovesPlayer(keyboard.leftArrowKey, new Vector2Int(1, 1), 1);
    }

    [Test]
    public void GameManagerStartInitializesTurnManagerBoardAndPlayer()
    {
        GameObject gameManagerObject = new GameObject("GameManager");
        GameObject boardObject = CreateBoardObject(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());
        Component gameManager = gameManagerObject.AddComponent(FindGameManagerType());

        SetField(gameManager, "BoardManager", boardManager);
        SetField(gameManager, "PlayerController", playerController);

        try
        {
            Invoke(gameManager, "Start");

            Assert.That(GetProperty(gameManager, "TurnManager"), Is.Not.Null);
            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(Invoke(boardManager, "GetCellData", new Vector2Int(1, 1)), Is.Not.Null);
        }
        finally
        {
            ResetGameManagerInstance();
            UnityEngine.Object.DestroyImmediate(gameManagerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
        }
    }

    [Test]
    public void TurnManagerStartsAtOneAndTicksUp()
    {
        object turnManager = Activator.CreateInstance(FindTurnManagerType());

        Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(1));

        Invoke(turnManager, "Tick");

        Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(2));
    }

    [Test]
    public void MainSceneUsesTutorialWallTilesAndSingleGroundTile()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Single);
        Component boardManager = Resources.FindObjectsOfTypeAll(FindBoardManagerType())
            .OfType<Component>()
            .Single(component => component.gameObject.scene == scene);

        Tile[] groundTiles = (Tile[])GetField(boardManager, "GroundTiles");
        Tile[] wallTiles = (Tile[])GetField(boardManager, "WallTiles");

        Assert.That(groundTiles, Is.EqualTo(new[] { LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_39.asset") }));
        Assert.That(wallTiles, Is.EqualTo(new[]
        {
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_1.asset"),
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_2.asset"),
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_3.asset"),
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_4.asset"),
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_5.asset"),
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_6.asset"),
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_7.asset"),
            LoadTile("Assets/Tiles/Scavengers2_SpriteSheet_8.asset")
        }));
    }

    static Type FindBoardManagerType()
    {
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("BoardManager"))
            .FirstOrDefault(candidate => candidate != null);

        Assert.That(type, Is.Not.Null, "BoardManager type should be compiled into the project.");
        return type;
    }

    static Type FindPlayerControllerType()
    {
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("PlayerController"))
            .FirstOrDefault(candidate => candidate != null);

        Assert.That(type, Is.Not.Null, "PlayerController type should be compiled into the project.");
        return type;
    }

    static Type FindGameManagerType()
    {
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("GameManager"))
            .FirstOrDefault(candidate => candidate != null);

        Assert.That(type, Is.Not.Null, "GameManager type should be compiled into the project.");
        return type;
    }

    static Type FindTurnManagerType()
    {
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("TurnManager"))
            .FirstOrDefault(candidate => candidate != null);

        Assert.That(type, Is.Not.Null, "TurnManager type should be compiled into the project.");
        return type;
    }

    static GameObject CreateBoardWithCells(int width, int height, out Component boardManager)
    {
        GameObject boardObject = new GameObject("BoardManager");
        Grid grid = boardObject.AddComponent<Grid>();
        Type boardManagerType = FindBoardManagerType();
        Type cellDataType = boardManagerType.GetNestedType("CellData");
        boardManager = boardObject.AddComponent(boardManagerType);
        Array boardData = Array.CreateInstance(cellDataType, width, height);

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                object cellData = Activator.CreateInstance(cellDataType);
                SetField(cellData, "Passable", x > 0 && y > 0);
                boardData.SetValue(cellData, x, y);
            }
        }

        SetField(boardManager, "Width", width);
        SetField(boardManager, "Height", height);
        SetField(boardManager, "m_Grid", grid);
        SetField(boardManager, "m_BoardData", boardData);

        return boardObject;
    }

    static GameObject CreateBoardObject(int width, int height, out Component boardManager)
    {
        GameObject boardObject = new GameObject("BoardManager");
        boardObject.AddComponent<Grid>();

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(boardObject.transform);
        tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();

        Tile groundTile = ScriptableObject.CreateInstance<Tile>();
        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        boardManager = boardObject.AddComponent(FindBoardManagerType());

        SetField(boardManager, "Width", width);
        SetField(boardManager, "Height", height);
        SetField(boardManager, "GroundTiles", new[] { groundTile });
        SetField(boardManager, "WallTiles", new[] { wallTile });

        return boardObject;
    }

    static GameObject CreateGameManager(out Component gameManager, out object turnManager)
    {
        GameObject gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent(FindGameManagerType());
        turnManager = Activator.CreateInstance(FindTurnManagerType());
        SetStaticProperty(FindGameManagerType(), "Instance", gameManager);
        SetProperty(gameManager, "TurnManager", turnManager);

        return gameManagerObject;
    }

    static void ResetGameManagerInstance()
    {
        SetStaticProperty(FindGameManagerType(), "Instance", null);
    }

    void AssertArrowKeyMovesPlayer(KeyControl key, Vector2Int expectedCell, int expectedTurnCount)
    {
        GameObject gameManagerObject = CreateGameManager(out Component gameManager, out object turnManager);
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));

            Press(key);
            Invoke(playerController, "Update");
            Release(key);

            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(expectedCell));
            Assert.That(GetField(turnManager, "m_TurnCount"), Is.EqualTo(expectedTurnCount));
        }
        finally
        {
            ResetGameManagerInstance();
            UnityEngine.Object.DestroyImmediate(gameManagerObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    static Tile LoadTile(string path)
    {
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        Assert.That(tile, Is.Not.Null, $"{path} should exist.");
        return tile;
    }

    static object GetField(object instance, string fieldName)
    {
        return instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(instance);
    }

    static void SetField(object instance, string fieldName, object value)
    {
        instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(instance, value);
    }

    static object GetProperty(object instance, string propertyName)
    {
        return instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(instance);
    }

    static void SetProperty(object instance, string propertyName, object value)
    {
        instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(instance, value);
    }

    static void SetStaticProperty(Type type, string propertyName, object value)
    {
        type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(null, value);
    }

    static object Invoke(object instance, string methodName, params object[] parameters)
    {
        return instance.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(instance, parameters);
    }
}
