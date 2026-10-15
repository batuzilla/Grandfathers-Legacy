using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }
    public static readonly HashSet<Vector2Int> WallCells = new HashSet<Vector2Int>();

    public bool Ready { get; private set; }

    public LayerMask blockedMask;
    public Vector2Int min = new Vector2Int(-30, -22);
    public Vector2Int max = new Vector2Int(30, 22);
    public float checkSize = 1.3f;
    public float wallCost = 6f;

    bool[] blocked;
    int w, h;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        WallCells.Clear();
    }

    public static Vector2Int WorldCell(Vector2 p)
    {
        return new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
    }

    public static void AddWall(Vector2 pos) { WallCells.Add(WorldCell(pos)); }
    public static void RemoveWall(Vector2 pos) { WallCells.Remove(WorldCell(pos)); }

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        Build();
    }

    void Build()
    {
        w = max.x - min.x;
        h = max.y - min.y;
        blocked = new bool[w * h];
        Vector2 size = new Vector2(checkSize, checkSize);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Vector2 c = new Vector2(min.x + x + 0.5f, min.y + y + 0.5f);
                blocked[y * w + x] = Physics2D.OverlapBox(c, size, 0f, blockedMask) != null;
            }
        Ready = true;
    }

    Vector2Int ToCell(Vector2 p)
    {
        return new Vector2Int(Mathf.FloorToInt(p.x) - min.x, Mathf.FloorToInt(p.y) - min.y);
    }

    Vector2 ToWorld(int id)
    {
        return new Vector2(min.x + id % w + 0.5f, min.y + id / w + 0.5f);
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < w && y < h;
    }

    bool Free(int x, int y)
    {
        return InBounds(x, y) && !blocked[y * w + x];
    }

    bool IsWall(int x, int y)
    {
        return WallCells.Contains(new Vector2Int(min.x + x, min.y + y));
    }

    Vector2Int NearestFree(Vector2Int c)
    {
        for (int r = 1; r <= 5; r++)
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                    if (Free(c.x + dx, c.y + dy)) return new Vector2Int(c.x + dx, c.y + dy);
        return c;
    }

    public List<Vector2> FindPath(Vector2 from, Vector2 to)
    {
        if (!Ready) return null;
        Vector2Int s = ToCell(from);
        Vector2Int g = ToCell(to);
        if (!InBounds(s.x, s.y) || !InBounds(g.x, g.y)) return null;
        if (!Free(g.x, g.y)) g = NearestFree(g);

        int n = w * h;
        int goalId = g.y * w + g.x;
        int startId = s.y * w + s.x;

        var gCost = new float[n];
        var parent = new int[n];
        var closed = new bool[n];
        for (int i = 0; i < n; i++) { gCost[i] = float.MaxValue; parent[i] = -1; }

        var open = new SortedSet<(float f, int id)>();
        gCost[startId] = 0f;
        open.Add((Heuristic(s.x, s.y, g.x, g.y), startId));

        while (open.Count > 0)
        {
            var cur = open.Min;
            open.Remove(cur);
            int id = cur.id;
            if (closed[id]) continue;
            closed[id] = true;

            if (id == goalId) return Build(parent, id, to);

            int cx = id % w, cy = id / w;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = cx + dx, ny = cy + dy;
                    if (!Free(nx, ny)) continue;

                    bool diagonal = dx != 0 && dy != 0;
                    if (diagonal)
                    {
                        if (!Free(cx + dx, cy) || !Free(cx, cy + dy)) continue;
                        if (IsWall(nx, ny) || IsWall(cx + dx, cy) || IsWall(cx, cy + dy)) continue;
                    }

                    int nid = ny * w + nx;
                    if (closed[nid]) continue;

                    float step = diagonal ? 1.414f : 1f;
                    if (IsWall(nx, ny)) step += wallCost;

                    float ng = gCost[id] + step;
                    if (ng < gCost[nid])
                    {
                        gCost[nid] = ng;
                        parent[nid] = id;
                        open.Add((ng + Heuristic(nx, ny, g.x, g.y), nid));
                    }
                }
        }
        return null;
    }

    float Heuristic(int x, int y, int gx, int gy)
    {
        int dx = Mathf.Abs(x - gx), dy = Mathf.Abs(y - gy);
        return (dx + dy) + (1.414f - 2f) * Mathf.Min(dx, dy);
    }

    List<Vector2> Build(int[] parent, int id, Vector2 target)
    {
        var path = new List<Vector2>();
        while (id != -1)
        {
            path.Add(ToWorld(id));
            id = parent[id];
        }
        path.Reverse();
        path[path.Count - 1] = target;
        return path;
    }

    void OnDrawGizmosSelected()
    {
        if (blocked == null) return;
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        for (int i = 0; i < blocked.Length; i++)
            if (blocked[i]) Gizmos.DrawCube(ToWorld(i), Vector3.one * 0.9f);
    }
}