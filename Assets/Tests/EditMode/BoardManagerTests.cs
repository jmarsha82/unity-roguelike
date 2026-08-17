using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class BoardManagerTests
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

        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());
        SetField(boardManager, "Player", playerController);

        try
        {
            boardManager.SendMessage("Start", SendMessageOptions.RequireReceiver);

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
            UnityEngine.Object.DestroyImmediate(playerObject);
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

        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());
        SetField(boardManager, "Player", playerController);

        try
        {
            boardManager.SendMessage("Start", SendMessageOptions.RequireReceiver);

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
            UnityEngine.Object.DestroyImmediate(playerObject);
        }
    }

    [Test]
    public void PlayerControllerMovesUpIntoPassableCell()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));

            PressAndUpdate(playerController, keyboard.upArrowKey);

            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 2)));
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerMovesRightIntoPassableCell()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));
            Vector3 startingPosition = playerObject.transform.position;

            PressAndUpdate(playerController, keyboard.rightArrowKey);

            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(playerObject.transform.position, Is.Not.EqualTo(startingPosition));

            Vector3 movedPosition = playerObject.transform.position;
            PressAndUpdate(playerController, keyboard.rightArrowKey);

            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(playerObject.transform.position, Is.EqualTo(movedPosition));
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerBlocksDownIntoImpassableCell()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));
            Vector3 startingPosition = playerObject.transform.position;

            PressAndUpdate(playerController, keyboard.downArrowKey);

            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(playerObject.transform.position, Is.EqualTo(startingPosition));
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerBlocksLeftIntoImpassableCell()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
        GameObject boardObject = CreateBoardWithCells(3, 3, out Component boardManager);
        GameObject playerObject = new GameObject("Player");
        Component playerController = playerObject.AddComponent(FindPlayerControllerType());

        try
        {
            Invoke(playerController, "Spawn", boardManager, new Vector2Int(1, 1));
            Vector3 startingPosition = playerObject.transform.position;

            PressAndUpdate(playerController, keyboard.leftArrowKey);

            Assert.That(GetField(playerController, "m_CellPosition"), Is.EqualTo(new Vector2Int(1, 1)));
            Assert.That(playerObject.transform.position, Is.EqualTo(startingPosition));
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
    }

    [Test]
    public void PlayerControllerIgnoresFramesWithoutInput()
    {
        Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
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
        }
        finally
        {
            InputSystem.RemoveDevice(keyboard);
            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(boardObject);
        }
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

    static void PressAndUpdate(Component playerController, KeyControl key)
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(key.keyCode));
        InputSystem.Update();
        Invoke(playerController, "Update");

        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        InputSystem.Update();
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

    static object Invoke(object instance, string methodName, params object[] parameters)
    {
        return instance.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(instance, parameters);
    }
}
