#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CleanupMissingScripts
{
    [MenuItem("Tools/Project/Clean Missing Scripts In Prefabs")]
    public static void CleanMissingScriptsInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int totalRemoved = 0;
        int processed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            int removed = 0;
            foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
            {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }

            if (removed > 0)
            {
                totalRemoved += removed;
                EditorUtility.SetDirty(prefab);
            }
            processed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"CleanupMissingScripts: Removed {totalRemoved} missing script component(s) across {processed} prefab(s).");
    }
}
#endif