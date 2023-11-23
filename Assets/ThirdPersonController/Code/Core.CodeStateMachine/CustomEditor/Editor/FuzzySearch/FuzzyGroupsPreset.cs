using System;
using TypeNamespaceTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Create FuzzyGroupsPreset", fileName = "FuzzyGroupsPreset", order = 0),
 FilePath("Assets/ThirdPersonController/Code/Core.CodeStateMachine/CustomEditor/Editor/FuzzySearch/FuzzyGroupsPreset.asset",FilePathAttribute.Location.ProjectFolder)]
public class FuzzyGroupsPreset : ScriptableSingleton<FuzzyGroupsPreset>
{
    [Serializable] 
    private struct Element
    {
        [FormerlySerializedAs("namespace_")] public string Namespace;
        [FormerlySerializedAs("contentName")] public string ContentName;
        [FormerlySerializedAs("icon")] public Texture2D Icon;
    }

    [FormerlySerializedAs("Elements")] [SerializeField] private Element[] elements;

    public CategoryTree[] GetCategoryTrees(CategoryTree root)
    {
        var result = new CategoryTree[elements.Length];
        for (int i = 0; i < elements.Length; i++)
        {
            ref var element = ref elements[i];
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