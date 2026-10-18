using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MenuSetup
{
    const string MenuPath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Tools/Ana Menü: Sahneyi Kur")]
    static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string gamePath = null;
        foreach (var guid in AssetDatabase.FindAssets("SampleScene t:Scene"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(p) == "SampleScene") { gamePath = p; break; }
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.13f, 1f);
        }

        var go = new GameObject("MainMenu");
        ObjectFactory.AddComponent<MainMenu>(go);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, MenuPath);

        var list = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(MenuPath, true) };
        if (gamePath != null) list.Add(new EditorBuildSettingsScene(gamePath, true));
        EditorBuildSettings.scenes = list.ToArray();

        EditorUtility.DisplayDialog("Ana Menü", "MainMenu sahnesi kuruldu ve Build Settings'e eklendi (0: MainMenu, 1: SampleScene).", "Tamam");
    }
}
