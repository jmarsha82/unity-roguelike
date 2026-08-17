using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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

    static Tile LoadTile(string path)
    {
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        Assert.That(tile, Is.Not.Null, $"{path} should exist.");
        return tile;
    }

    static object GetField(Component component, string fieldName)
    {
        return component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public).GetValue(component);
    }

    static void SetField(Component component, string fieldName, object value)
    {
        component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public).SetValue(component, value);
    }
}
