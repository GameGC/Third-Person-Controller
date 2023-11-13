using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TransformExtensions
{
    [MenuItem("CONTEXT/Transform/Place pivot at child's Center", false)]
    public static void PlacePivotOnChildsCenter(MenuCommand command)
    {
        var transform = command.context as Transform;
        if(PrefabUtility.IsPartOfAnyPrefab(transform.gameObject))
            PrefabUtility.UnpackPrefabInstance(transform.gameObject,PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
        
        
        var renderers = transform.GetComponentsInChildren<Renderer>();
        var bounds = new Bounds
        {
            center = renderers[0].bounds.center
        };
        foreach (var componentsInChild in renderers)
        {
            bounds.Encapsulate(componentsInChild.bounds);
        }

        var childList = new List<Transform>(capacity: transform.childCount);
        foreach (Transform child in transform)
        {
            childList.Add(child);
            child.SetParent(null);
        }

        transform.position = bounds.center;
        foreach (var child in childList)
        {
            child.SetParent(transform);
        }
    }
    
    [MenuItem("CONTEXT/Transform/Rename By Instance Id", false)]
    public static void RenameToInstanceID()
    {
        foreach (var x in Selection.gameObjects)
        {
            x.name = $"GameObject {x.GetInstanceID()}";
            EditorUtility.SetDirty(x);
        }
    }
}
