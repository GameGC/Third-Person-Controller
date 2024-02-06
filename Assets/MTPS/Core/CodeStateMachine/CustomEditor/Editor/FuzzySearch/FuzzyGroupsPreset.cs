using System;
using TypeNamespaceTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Create FuzzyGroupsPreset", fileName = "FuzzyGroupsPreset", order = 0),
 FilePath("Assets/MTPS/Core/CodeStateMachine/CustomEditor/Editor/FuzzySearch/FuzzyGroupsPreset.asset",FilePathAttribute.Location.ProjectFolder)]
public class FuzzyGroupsPreset : ScriptableSingleton<FuzzyGroupsPreset>
{
    [Serializable] 
    private struct Element
    {
        [FormerlySerializedAs("namespace_")] public string Namespace;
        [FormerlySerializedAs("contentName")] public string ContentName;
        [FormerlySerializedAs("icon")] public Texture2D Icon;
    }

    [FormerlySerializedAs("elements")] [FormerlySerializedAs("Elements")] [SerializeField] private Element[] features;

    [SerializeField] private Element[] transitions;
    
    public CategoryTree[] GetFeatureTrees(CategoryTree root)
    {
        var result = new CategoryTree[features.Length];
        for (int i = 0; i < features.Length; i++)
        {
            ref var element = ref features[i];
            result[i] = new CategoryTree()
            {
                Namespace = element.Namespace,
                Content = element.Icon
                    ? EditorGUIUtility.TrTextContentWithIcon(element.ContentName, element.Icon)
                    : EditorGUIUtility.TrTextContent(element.ContentName),
                Parent = root
            };
        }

        return result;
    }
    
    public CategoryTree[] GetTransitionTrees(CategoryTree root)
    {
        var result = new CategoryTree[transitions.Length];
        for (int i = 0; i < transitions.Length; i++)
        {
            ref var element = ref transitions[i];
            result[i] = new CategoryTree()
            {
                Namespace = element.Namespace,
                Content = element.Icon
                    ? EditorGUIUtility.TrTextContentWithIcon(element.ContentName, element.Icon)
                    : EditorGUIUtility.TrTextContent(element.ContentName),
                Parent = root
            };
        }

        return result;
    }
}