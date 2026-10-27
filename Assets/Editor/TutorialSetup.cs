using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TutorialSetup
{
    const string BearFolder = "Assets/Sprites/Bear";
    const string CharacterRoot = "Assets/Sprites/Ninja Adventure - Asset Pack/Actor/Character/";

    [MenuItem("Tools/Tutorial ve Ayı Sahnesi: Kur")]
    static void Setup()
    {
        if (!Directory.Exists(BearFolder))
        {
            EditorUtility.DisplayDialog("Tutorial", "Assets/Sprites/Bear klasörü yok. BearSprites.zip içindeki Bear klasörünü Assets/Sprites altına kopyala, sonra menüyü tekrar çalıştır.", "Tamam");
            return;
        }

        foreach (var path in Directory.GetFiles(BearFolder, "*.png"))
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

        var intro = Object.FindFirstObjectByType<IntroSequence>();
        if (intro == null)
        {
            EditorUtility.DisplayDialog("Tutorial", "Sahnede IntroSequence bulunamadı. SampleScene açık mı?", "Tamam");
            return;
        }

        Undo.RecordObject(intro, "Tutorial kurulumu");

        var tut = intro.GetComponent<TutorialSequence>();
        if (tut == null) tut = ObjectFactory.AddComponent<TutorialSequence>(intro.gameObject);
        intro.tutorial = tut;

        if (intro.dialogue == null) intro.dialogue = intro.GetComponent<DialogueUI>();
        if (intro.spawner == null) intro.spawner = Object.FindFirstObjectByType<WaveSpawner>();

        if (intro.dedeSheet == null) intro.dedeSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(CharacterRoot + "Master/SpriteSheet.png");
        if (intro.dedeFace == null) intro.dedeFace = AssetDatabase.LoadAssetAtPath<Sprite>(CharacterRoot + "Master/Faceset.png");
        if (intro.playerFace == null) intro.playerFace = AssetDatabase.LoadAssetAtPath<Sprite>(CharacterRoot + "Knight/Faceset.png");

        if (intro.throwItems == null || intro.throwItems.Length == 0)
        {
            var items = new List<GameObject>();
            var bow = GameObject.Find("Ground_Bow");
            var arrow = GameObject.Find("Arrow_0");
            if (bow != null) items.Add(bow);
            if (arrow != null) items.Add(arrow);
            intro.throwItems = items.ToArray();
        }

        intro.bearFace = EditorClipFinder.FindSprite("Bear_Faceset");
        intro.bearSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(BearFolder + "/Bear_SpriteSheet.png");
        EditorUtility.SetDirty(intro);

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<PlayerBowPose>() == null)
        {
            var pose = ObjectFactory.AddComponent<PlayerBowPose>(player);
            pose.sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(CharacterRoot + "Knight/SpriteSheet.png");
            EditorUtility.SetDirty(pose);
        }

        EditorSceneManager.MarkSceneDirty(intro.gameObject.scene);
        EditorUtility.DisplayDialog("Tutorial", "Intro, tutorial, ayı sahnesi ve yay pozu bağlandı. Sahneyi kaydet (Ctrl+S).", "Tamam");
    }
}
