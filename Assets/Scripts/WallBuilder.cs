using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallBuilder : MonoBehaviour
{
    public Tilemap wallTilemap;
    public WallSegment wallPrefab;

    static readonly string[] wallSpriteNames = { "tile_0100", "tile_0061" };

    void Awake()
    {
        var names = new HashSet<string>(wallSpriteNames);
        var cells = new List<Vector3Int>();

        foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
        {
            var sp = wallTilemap.GetSprite(pos);
            if (sp != null && names.Contains(sp.name)) cells.Add(pos);
        }

        foreach (var pos in cells)
        {
            var sp = wallTilemap.GetSprite(pos);
            var seg = Instantiate(wallPrefab, wallTilemap.GetCellCenterWorld(pos), Quaternion.identity, transform);
            seg.GetComponent<SpriteRenderer>().sprite = sp;
            wallTilemap.SetTile(pos, null);
        }

        Debug.Log(cells.Count + " duvar parçası oluşturuldu.");
    }
}