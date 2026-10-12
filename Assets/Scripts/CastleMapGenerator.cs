using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CastleMapGenerator : MonoBehaviour
{
    [Header("Tilemaps (decorMap otomatik oluşur)")]
    public Tilemap groundMap;
    public Tilemap obstacleMap;
    public Tilemap decorMap;

    [Header("Sprite deposu (Tools > Kale: Sprite'ları Yükle ile dolar)")]
    public Sprite[] sprites;

    [Header("Zemin (tile numaraları, tekrar = daha sık çıkar)")]
    public int[] grassTiles = { 0, 0, 0, 0, 0, 1 };
    public int flowerTile = 2;
    [Range(0f, 0.2f)] public float flowerChance = 0.03f;
    public int[] pathTiles = { 25, 25, 25, 25, 41, 42 };
    public int[] courtyardTiles = { 109, 109, 109, 109, 77 };

    [Header("Kale")]
    public int wallHorizontalTile = 100;
    public int wallVerticalTile = 61;
    public int towerTile = 102;
    public int gateTile = 78;
    public DamageableWall wallPrefab;   // opsiyonel, boş kalabilir

    [Header("Avlu eşyaları (tile_XXXX veya item adı)")]
    public string[] courtyardProps = { "CrateEmpty", "Bag", "tile_0095", "tile_0107" };
    public int courtyardPropCount = 8;

    [Header("Dekor")]
    public int[] treeTiles = { 4, 4, 28, 16, 16, 3, 27 };
    public int[] bushTiles = { 5, 17 };
    public int mushroomTile = 29;
    [Range(0f, 0.3f)] public float treeDensity = 0.05f;
    [Range(0f, 0.3f)] public float bushDensity = 0.03f;
    [Range(0f, 0.1f)] public float mushroomChance = 0.01f;
    public int borderThickness = 3;

    [Header("Boyutlar")]
    public int mapWidth = 60;
    public int mapHeight = 44;
    public int castleWidth = 20;
    public int castleHeight = 16;
    public int gateWidth = 3;

    [Header("Diğer")]
    public int spawnPointCount = 6;
    public int seed = 0;   // 0 = her seferinde farklı harita

    public Vector3 CastleCenter { get; private set; }
    public List<Transform> SpawnPoints { get; private set; } = new List<Transform>();
    public List<DamageableWall> Walls { get; private set; } = new List<DamageableWall>();

    Dictionary<string, Sprite> spriteByName;
    Dictionary<string, Tile> tileCache;
    HashSet<Vector2Int> reserved;

    int MinX => -mapWidth / 2;
    int MaxX => mapWidth / 2;
    int MinY => -mapHeight / 2;
    int MaxY => mapHeight / 2;
    int Left => -castleWidth / 2;
    int Right => castleWidth / 2 - 1;
    int Bottom => -castleHeight / 2;
    int Top => castleHeight / 2 - 1;

    void Start() => Generate();

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (seed != 0) Random.InitState(seed);
        PrepareLookup();
        EnsureDecorMap();
        Clear();
        reserved = new HashSet<Vector2Int>();

        BuildGround();
        BuildCastle();
        BuildPath();
        BuildCourtyardProps();
        BuildDecor();
        BuildSpawnPoints();
    }

    // ---------- Hazırlık ----------

    void PrepareLookup()
    {
        spriteByName = new Dictionary<string, Sprite>();
        tileCache = new Dictionary<string, Tile>();
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("Sprite deposu boş! Tools > Kale: Sprite'ları Yükle menüsünü çalıştır.");
            return;
        }
        foreach (var s in sprites)
            if (s != null) spriteByName.TryAdd(s.name, s);
    }

    void EnsureDecorMap()
    {
        if (decorMap != null) return;
        var go = new GameObject("Decor");
        go.transform.SetParent(groundMap.transform.parent, false);
        decorMap = go.AddComponent<Tilemap>();
        var r = go.AddComponent<TilemapRenderer>();
        r.sortingOrder = 5;
        go.AddComponent<TilemapCollider2D>();
    }

    void Clear()
    {
        groundMap.ClearAllTiles();
        obstacleMap.ClearAllTiles();
        decorMap.ClearAllTiles();
        foreach (var w in Walls) if (w) DestroyObj(w.gameObject);
        foreach (var s in SpawnPoints) if (s) DestroyObj(s.gameObject);
        Walls.Clear();
        SpawnPoints.Clear();
    }

    static void DestroyObj(GameObject go)
    {
        if (Application.isPlaying) Destroy(go); else DestroyImmediate(go);
    }

    // ---------- Tile yardımcıları ----------

    static string Name(int n) => "tile_" + n.ToString("0000");

    Tile GetTile(string spriteName, Tile.ColliderType ct)
    {
        string key = spriteName + "|" + ct;
        if (tileCache.TryGetValue(key, out var cached)) return cached;

        spriteByName.TryGetValue(spriteName, out var sp);
        if (sp == null)
        {
            Debug.LogWarning("Sprite bulunamadı: " + spriteName);
            tileCache[key] = null;
            return null;
        }
        var t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = sp;
        t.colliderType = ct;
        t.name = spriteName;
        tileCache[key] = t;
        return t;
    }

    void Put(Tilemap map, int x, int y, string spriteName, Tile.ColliderType ct)
    {
        var t = GetTile(spriteName, ct);
        if (t != null) map.SetTile(new Vector3Int(x, y, 0), t);
    }

    void Put(Tilemap map, int x, int y, int tileNo, Tile.ColliderType ct) => Put(map, x, y, Name(tileNo), ct);

    static int Pick(int[] arr) => arr[Random.Range(0, arr.Length)];

    // ---------- Zemin ----------

    void BuildGround()
    {
        for (int x = MinX; x < MaxX; x++)
            for (int y = MinY; y < MaxY; y++)
            {
                int no = Random.value < flowerChance ? flowerTile : Pick(grassTiles);
                Put(groundMap, x, y, no, Tile.ColliderType.None);
            }
    }

    // ---------- Kale ----------

    void BuildCastle()
    {
        CastleCenter = new Vector3(0.5f, 0.5f, 0);

        // Avlu zemini
        for (int x = Left + 1; x < Right; x++)
            for (int y = Bottom + 1; y < Top; y++)
                Put(groundMap, x, y, Pick(courtyardTiles), Tile.ColliderType.None);

        // Kale + çevresi dekor almasın
        for (int x = Left - 2; x <= Right + 2; x++)
            for (int y = Bottom - 2; y <= Top + 2; y++)
                reserved.Add(new Vector2Int(x, y));

        int gateLeft = -gateWidth / 2, gateRight = gateLeft + gateWidth - 1;

        for (int x = Left + 1; x < Right; x++)
        {
            PlaceWall(x, Top, wallHorizontalTile);
            if (x >= gateLeft && x <= gateRight)
                Put(obstacleMap, x, Bottom, gateTile, Tile.ColliderType.None); // kapı geçilebilir
            else
                PlaceWall(x, Bottom, wallHorizontalTile);
        }
        for (int y = Bottom + 1; y < Top; y++)
        {
            PlaceWall(Left, y, wallVerticalTile);
            PlaceWall(Right, y, wallVerticalTile);
        }

        // Köşe kuleleri
        Put(obstacleMap, Left, Bottom, towerTile, Tile.ColliderType.Grid);
        Put(obstacleMap, Right, Bottom, towerTile, Tile.ColliderType.Grid);
        Put(obstacleMap, Left, Top, towerTile, Tile.ColliderType.Grid);
        Put(obstacleMap, Right, Top, towerTile, Tile.ColliderType.Grid);
    }

    void PlaceWall(int x, int y, int tileNo)
    {
        if (wallPrefab != null)
        {
            var w = Instantiate(wallPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, transform);
            Walls.Add(w);
        }
        else Put(obstacleMap, x, y, tileNo, Tile.ColliderType.Grid);
    }

    // ---------- Kıvrımlı yol ----------

    void BuildPath()
    {
        int gateLeft = -gateWidth / 2;
        int offset = 0;
        for (int y = Bottom - 1; y >= MinY; y--)
        {
            float r = Random.value;
            if (r < 0.2f) offset--;
            else if (r < 0.4f) offset++;
            offset = Mathf.Clamp(offset, -6, 6);

            for (int x = gateLeft + offset; x < gateLeft + offset + gateWidth; x++)
                Put(groundMap, x, y, Pick(pathTiles), Tile.ColliderType.None);

            for (int x = gateLeft + offset - 1; x <= gateLeft + offset + gateWidth; x++)
                reserved.Add(new Vector2Int(x, y));
        }
    }

    // ---------- Avlu eşyaları ----------

    void BuildCourtyardProps()
    {
        if (courtyardProps == null || courtyardProps.Length == 0) return;
        var used = new HashSet<Vector2Int>();
        int placed = 0, tries = 0;
        int gateHalf = gateWidth / 2 + 1;

        while (placed < courtyardPropCount && tries < courtyardPropCount * 20)
        {
            tries++;
            int x = Random.Range(Left + 2, Right - 1);
            int y = Random.Range(Bottom + 2, Top - 1);
            if (Mathf.Abs(x) <= gateHalf) continue;      // kapıdan içeri yol boş kalsın
            var p = new Vector2Int(x, y);
            if (!used.Add(p)) continue;

            string prop = courtyardProps[Random.Range(0, courtyardProps.Length)];
            Put(decorMap, x, y, prop, Tile.ColliderType.Grid);
            placed++;
        }
    }

    // ---------- Dekor ----------

    void BuildDecor()
    {
        for (int x = MinX; x < MaxX; x++)
            for (int y = MinY; y < MaxY; y++)
            {
                if (reserved.Contains(new Vector2Int(x, y))) continue;

                int distEdge = Mathf.Min(Mathf.Min(x - MinX, MaxX - 1 - x), Mathf.Min(y - MinY, MaxY - 1 - y));
                float r = Random.value;

                if (distEdge < borderThickness)
                {
                    if (r < 0.9f) Put(decorMap, x, y, Pick(treeTiles), Tile.ColliderType.Grid);
                }
                else if (r < treeDensity)
                    Put(decorMap, x, y, Pick(treeTiles), Tile.ColliderType.Grid);
                else if (r < treeDensity + bushDensity)
                    Put(decorMap, x, y, Pick(bushTiles), Tile.ColliderType.Grid);
                else if (r < treeDensity + bushDensity + mushroomChance)
                    Put(decorMap, x, y, mushroomTile, Tile.ColliderType.None);
            }
    }

    // ---------- Yaratık doğma noktaları ----------

    void BuildSpawnPoints()
    {
        for (int i = 0; i < spawnPointCount; i++)
        {
            float angle = (360f / spawnPointCount) * i * Mathf.Deg2Rad;
            float rx = (mapWidth / 2f) - borderThickness - 2f;
            float ry = (mapHeight / 2f) - borderThickness - 2f;
            var go = new GameObject($"SpawnPoint_{i}");
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry, 0);
            SpawnPoints.Add(go.transform);
        }
    }
}