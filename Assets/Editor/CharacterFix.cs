using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CharacterFix
{
    const string EnemyPrefab = "Assets/Prefabs/Enemy.prefab";
    const string BearSheet = "Assets/Sprites/Bear/Bear_SpriteSheet.png";

    [MenuItem("Tools/Karakter ve Ayı: Düzelt")]
    static void Fix()
    {
        string report = "";

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var box = player.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                Undo.RecordObject(box, "Oyuncu collider");
                box.size = new Vector2(0.6f, 0.5f);
                box.offset = new Vector2(0f, -0.25f);
                EditorUtility.SetDirty(box);
                EditorSceneManager.MarkSceneDirty(player.scene);
                report += "Oyuncu collider küçültüldü (tek karelik aralıklardan geçer).\n";
            }
        }
        else report += "Player bulunamadı.\n";

        var sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(BearSheet);
        if (sheet == null)
        {
            report += "Assets/Sprites/Bear/Bear_SpriteSheet.png bulunamadı, ayı animasyonu eklenmedi.\n";
        }
        else
        {
            var root = PrefabUtility.LoadPrefabContents(EnemyPrefab);
            var anim = root.GetComponent<DirectionalSheetAnimator>();
            if (anim == null) anim = root.AddComponent<DirectionalSheetAnimator>();
            anim.sheet = sheet;
            anim.cell = 16;
            anim.walkFrames = 4;
            anim.downColumn = 0;
            anim.leftColumn = 1;
            anim.upColumn = 2;
            anim.rightColumn = 3;
            PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefab);
            PrefabUtility.UnloadPrefabContents(root);
            report += "Ayı prefab'ına yürüme ve saldırı animasyonu eklendi.\n";
        }

        EditorUtility.DisplayDialog("Karakter ve Ayı", report + "\nSahneyi kaydet (Ctrl+S).", "Tamam");
    }
}
