using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ShopSetup
{
    [MenuItem("Tools/Market: Tezgahı Kur")]
    static void Build()
    {
        foreach (var path in Directory.GetFiles("Assets/Sprites/Shop", "*.png"))
        {
            var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        var existing = Object.FindFirstObjectByType<ShopStation>();
        if (existing != null)
        {
            Selection.activeObject = existing.gameObject;
            EditorUtility.DisplayDialog("Market", "Sahnede zaten bir Market var, seçtim.", "Tamam");
            return;
        }

        var go = new GameObject("Market");
        Undo.RegisterCreatedObjectUndo(go, "Market");
        go.transform.position = new Vector3(4.3f, 3f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = EditorClipFinder.FindSprite("Stall");
        sr.sortingOrder = 4;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.6f, 0.8f);
        col.offset = new Vector2(0f, -0.55f);

        ObjectFactory.AddComponent<ShopStation>(go);

        var story = GameObject.Find("Story");
        if (story != null && story.GetComponent<WaveHUD>() == null)
            ObjectFactory.AddComponent<WaveHUD>(story);

        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeObject = go;
        EditorUtility.DisplayDialog("Market", "Market tezgahı kuruldu. Sahnede istediğin yere sürükleyebilirsin. Kaydetmeyi unutma (Ctrl+S).", "Tamam");
    }
}
