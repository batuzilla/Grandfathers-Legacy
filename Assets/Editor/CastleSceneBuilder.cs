using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CastleSceneBuilder
{
    [MenuItem("Tools/Kale Sahnesini Kur")]
    static void Build()
    {
        var gridGO = new GameObject("Grid");
        gridGO.AddComponent<Grid>().cellSize = new Vector3(1, 1, 0);

        var ground = CreateMap("Ground", gridGO.transform, -10, false);
        var obstacles = CreateMap("Obstacles", gridGO.transform, 0, true);

        var gen = new GameObject("CastleMapGenerator").AddComponent<CastleMapGenerator>();
        gen.groundMap = ground;
        gen.obstacleMap = obstacles;

        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 12;
            cam.transform.position = new Vector3(0, 0, -10);
        }

        Selection.activeGameObject = gen.gameObject;
        Debug.Log("Sahne kuruldu. CastleMapGenerator'a tile'ları ata ve Play'e bas.");
    }

    static Tilemap CreateMap(string name, Transform parent, int order, bool collider)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var tm = go.AddComponent<Tilemap>();
        var r = go.AddComponent<TilemapRenderer>();
        r.sortingOrder = order;
        if (collider)
        {
            go.AddComponent<TilemapCollider2D>();
        }
        return tm;
    }
}