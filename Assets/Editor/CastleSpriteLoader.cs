using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CastleSpriteLoader
{
    [MenuItem("Tools/Kale: Sprite'ları Yükle")]
    static void Load()
    {
        var gen = Object.FindFirstObjectByType<CastleMapGenerator>();
        if (gen == null)
        {
            Debug.LogError("Sahnede CastleMapGenerator yok.");
            return;
        }

        var props = new HashSet<string>(gen.courtyardProps ?? new string[0]);
        var list = new List<Sprite>();
        var seen = new HashSet<string>();
        int scanned = 0;

        // 1) Sprite asset'leri (dilimlenmiş PNG'ler, Items klasörü vb.)
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (var s in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
            {
                scanned++;
                if (Wanted(s.name, props) && seen.Add(s.name)) list.Add(s);
            }
        }

        // 2) Tile asset'leri (Tile Palette'in kaydettiği tile_XXXX dosyaları)
        foreach (var guid in AssetDatabase.FindAssets("t:Tile"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null || tile.sprite == null) continue;
            scanned++;
            if (Wanted(tile.name, props) && seen.Add(tile.name)) list.Add(tile.sprite);
        }

        gen.sprites = list.ToArray();
        EditorUtility.SetDirty(gen);
        Debug.Log($"{list.Count} sprite yüklendi. (Taranan: {scanned})");
    }

    static bool Wanted(string name, HashSet<string> props)
        => name.StartsWith("tile_") || props.Contains(name);
}