using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class MapPainter
{
    const string MapPath = "Assets/Map/map.json";
    const string TileDir = "Assets/Map/Tiles";

    [System.Serializable] public class Cell { public int x, y, t, c; }
    [System.Serializable] public class Anim { public string kind, sheet; public float x, y, cx, cy, hw, hh; }
    [System.Serializable] public class MapData { public Cell[] ground, obstacle, decor; public Anim[] anim; public float[] dede; }

    static readonly HashSet<string> missing = new HashSet<string>();

    [MenuItem("Tools/Harita: Kur (map.json)")]
    static void Build()
    {
        var json = AssetDatabase.LoadAssetAtPath<TextAsset>(MapPath);
        if (json == null)
        {
            Debug.LogError("Bulunamadi: " + MapPath + "  (map.json dosyasini Assets/Map klasorune koy)");
            return;
        }

        var data = JsonUtility.FromJson<MapData>(json.text);
        var sprites = LoadSprites();
        if (sprites.Count == 0)
        {
            Debug.LogError("tile_XXXX sprite'lari bulunamadi.");
            return;
        }

        missing.Clear();
        EnsureFolder("Assets/Map");
        EnsureFolder(TileDir);

        var ground = GetMap("Ground", -10, false);
        var obstacles = GetMap("Obstacles", 0, true);
        var decor = GetMap("Decor", 5, true);

        Paint(ground, data.ground, sprites, false);
        Paint(obstacles, data.obstacle, sprites, true);
        Paint(decor, data.decor, sprites, true);
        AssetDatabase.SaveAssets();

        BuildLife(data.anim);

        var target = GameObject.Find("CastleTarget");
        if (target != null && data.dede != null && data.dede.Length == 2)
            target.transform.position = new Vector3(data.dede[0], data.dede[1], 0f);

        if (missing.Count > 0) Debug.LogWarning("Eksik sprite: " + string.Join(", ", missing));
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Harita kuruldu. Sahneyi kaydet (Ctrl+S).");
    }

    static Dictionary<string, Sprite> LoadSprites()
    {
        var d = new Dictionary<string, Sprite>();
        foreach (var guid in AssetDatabase.FindAssets("tile_ t:Texture2D"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (var s in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                if (s.name.StartsWith("tile_")) d.TryAdd(s.name, s);
        }
        foreach (var guid in AssetDatabase.FindAssets("t:Tile"))
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(guid));
            if (tile != null && tile.sprite != null && tile.sprite.name.StartsWith("tile_"))
                d.TryAdd(tile.sprite.name, tile.sprite);
        }
        return d;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    static Tilemap GetMap(string name, int order, bool collider)
    {
        var tm = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(t => t.name == name && t.gameObject.activeInHierarchy);

        if (tm == null)
        {
            var grid = GameObject.Find("Grid");
            if (grid == null)
            {
                grid = new GameObject("Grid");
                grid.AddComponent<Grid>();
            }
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            tm = go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
        }

        if (collider && tm.GetComponent<TilemapCollider2D>() == null)
            tm.gameObject.AddComponent<TilemapCollider2D>();

        if (name == "Decor")
        {
            int layer = LayerMask.NameToLayer("Trees");
            if (layer >= 0) tm.gameObject.layer = layer;
        }
        return tm;
    }

    static Tile GetTile(string spriteName, bool solid, Sprite sprite)
    {
        string path = TileDir + "/" + spriteName + (solid ? "_solid" : "_free") + ".asset";
        var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (t == null)
        {
            t = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(t, path);
        }
        t.sprite = sprite;
        t.colliderType = solid ? Tile.ColliderType.Grid : Tile.ColliderType.None;
        EditorUtility.SetDirty(t);
        return t;
    }

    static void Paint(Tilemap tm, Cell[] cells, Dictionary<string, Sprite> sprites, bool allowCollider)
    {
        tm.ClearAllTiles();
        var pos = new List<Vector3Int>();
        var tiles = new List<TileBase>();
        foreach (var c in cells)
        {
            string n = "tile_" + c.t.ToString("0000");
            if (!sprites.TryGetValue(n, out var sp)) { missing.Add(n); continue; }
            pos.Add(new Vector3Int(c.x, c.y, 0));
            tiles.Add(GetTile(n, allowCollider && c.c == 1, sp));
        }
        tm.SetTiles(pos.ToArray(), tiles.ToArray());
        tm.CompressBounds();
        EditorUtility.SetDirty(tm);
    }

    static Texture2D LoadTex(string pathSuffix)
    {
        string file = Path.GetFileNameWithoutExtension(pathSuffix);
        foreach (var guid in AssetDatabase.FindAssets(file + " t:Texture2D"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(pathSuffix)) return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        Debug.LogWarning("Texture bulunamadi: " + pathSuffix);
        return null;
    }

    static void BuildLife(Anim[] anims)
    {
        var old = GameObject.Find("WorldLife");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("WorldLife").transform;

        var flag = LoadTex("Animated/Flag/FlagRed16x16.png");
        var plant = LoadTex("Animated/Plant/SpriteSheet16x16.png");
        var smoke = LoadTex("Smoke/Smoke/SpriteSheet.png");
        var butterfly = LoadTex("Monster/Butterfly/SpriteSheet.png");
        var sheets = new Dictionary<string, Texture2D>
        {
            { "cow", LoadTex("Animal/Cow/SpriteSheetWhite.png") },
            { "chickenWhite", LoadTex("Animal/Chicken/SpriteSheetWhite.png") },
            { "chickenBrown", LoadTex("Animal/Chicken/SpriteSheetBrown.png") },
        };

        foreach (var a in anims)
        {
            var go = new GameObject(a.kind);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(a.x, a.y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();

            switch (a.kind)
            {
                case "flag":
                    sr.sortingOrder = 6;
                    var f = go.AddComponent<SheetAnimator>();
                    f.sheet = flag; f.frames = 4; f.fps = 6f;
                    break;

                case "plant":
                    sr.sortingOrder = 1;
                    var p = go.AddComponent<SheetAnimator>();
                    p.sheet = plant; p.frames = 4; p.fps = 3f;
                    break;

                case "smoke":
                    Object.DestroyImmediate(sr);
                    go.AddComponent<SmokeEmitter>().sheet = smoke;
                    break;

                case "butterfly":
                    sr.sortingOrder = 8;
                    go.transform.localScale = Vector3.one * 0.6f;
                    go.AddComponent<DirectionalSheetAnimator>().sheet = butterfly;
                    go.GetComponent<DirectionalSheetAnimator>().fps = 10f;
                    go.AddComponent<ButterflyFlutter>();
                    break;

                case "animal":
                    sr.sortingOrder = 3;
                    var w = go.AddComponent<WanderAnimal>();
                    sheets.TryGetValue(a.sheet, out w.sheet);
                    w.flipWhenLeft = a.sheet != "cow";
                    w.areaCenter = new Vector2(a.cx, a.cy);
                    w.areaHalfSize = new Vector2(a.hw, a.hh);
                    break;
            }
        }
    }
}
