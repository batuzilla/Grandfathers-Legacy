using UnityEngine;

public static class EditorClipFinder
{
#if UNITY_EDITOR
    public static AudioClip Find(string fileName)
    {
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets(fileName + " t:AudioClip"))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == fileName)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return null;
    }
#endif
}
