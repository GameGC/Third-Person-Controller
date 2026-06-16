using UnityEngine.Animations.Rigging;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class FixMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void FindMissing()
    {
        int count = 0;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.IsValid()) continue;
            var components = go.GetComponents<Component>();
            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    Debug.LogError($"Missing script on: {go.name} (path: {GetPath(go)})", go);
                    count++;
                }
            }
        }
        Debug.Log($"Found {count} missing script references.");
    }

    [MenuItem("Tools/Remove Missing Scripts")]
    public static void RemoveMissing()
    {
        int count = 0;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.IsValid()) continue;
            var components = go.GetComponents<Component>();
            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    count++;
                    break;
                }
            }
        }
        Debug.Log($"Removed {count} missing script references.");
    }

    [MenuItem("Tools/Remove Null Rig Layers")]
    public static void RemoveNullRigLayers()
    {
        foreach (var rigBuilder in Resources.FindObjectsOfTypeAll<RigBuilder>())
        {
            if (!rigBuilder.gameObject.scene.IsValid()) continue;
            bool changed = false;
            for (int i = rigBuilder.layers.Count - 1; i >= 0; i--)
            {
                if (rigBuilder.layers[i].rig == null)
                {
                    rigBuilder.layers.RemoveAt(i);
                    changed = true;
                }
            }
            if (changed)
            {
                EditorUtility.SetDirty(rigBuilder);
                Debug.Log($"Removed null rig layers from: {rigBuilder.gameObject.name}");
            }
        }
    }

    private static string GetPath(GameObject go)
    {
        string path = go.name;
        var parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
