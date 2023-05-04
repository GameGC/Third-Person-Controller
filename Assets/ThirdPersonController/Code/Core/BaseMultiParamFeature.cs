using System;
using GameGC.Collections;
using ThirdPersonController.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[Serializable]
public abstract class BaseMultiParamFeature<T> : BaseFeature where T : ScriptableObject
{
    [SerializeField] private SDictionary<string, T> _paramVariants = new() {{"Default", ScriptableObject.CreateInstance<T>()}};

    protected T Value => _paramVariants["Default"];
}

[CustomPropertyDrawer(typeof(BaseMultiParamFeature<>),true)]
public class BaseMultiParamFeatureEditor : PropertyDrawerWithCustomData<BaseMultiParamFeatureEditor.Data>
{
    public struct Data
    {
        public ReorderableList List;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label)+ 100+100;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, Data customData)
    {
        var targetArray = property.FindPropertyRelative("_paramVariants").FindPropertyRelative("_keyValuePairs");

        if (customData.List == null)
        {
            customData.List = CreateDrawer(targetArray);
        }
        
        
        SerializedProperty iterator = property;
        bool next = iterator.NextVisible(true);
        next = iterator.NextVisible(false);

        if (next)
        {
            EditorGUI.LabelField(new Rect(position.x,position.y,position.width,18),"Shared:", EditorStyles.boldLabel);
            position.y += 18f;
        }
        
        while (next)
        {
            EditorGUI.PropertyField(position,iterator, true);
            position.y += EditorGUI.GetPropertyHeight(iterator);
            
            next = iterator.NextVisible(false);
        }
      
        var target =targetArray.GetArrayElementAtIndex(customData.List.selectedIndices[0]).FindPropertyRelative("Value");
        customData.List.DoList(position);
        position.y += customData.List.GetHeight();
        
        
        EditorGUI.LabelField(new Rect(position.x,position.y,position.width,18),
            targetArray.GetArrayElementAtIndex(customData.List.selectedIndices[0]).FindPropertyRelative("Key").stringValue+":",
            EditorStyles.boldLabel
            );
        position.y += 18f;
        
        DrawAllChildProperties(target.objectReferenceValue, ref position);
    }

    private void DrawAllChildProperties(Object target, ref Rect rect)
    {
        var editor = new SerializedObject(target as ScriptableObject);
        editor.Update();

        SerializedProperty iterator = editor.GetIterator();
        bool next = iterator.NextVisible(true);
        next = iterator.NextVisible(true);
        
        while (next)
        {
            EditorGUI.PropertyField(rect,iterator, true);
            rect.y += EditorGUI.GetPropertyHeight(iterator);
            
            next = iterator.NextVisible(false);
        }

        editor.ApplyModifiedProperties();
    }

    private ReorderableList CreateDrawer(SerializedProperty property)
    {
        //ReorderableList.defaultBehaviours.DrawElement();
        ReorderableList readable = null;
        readable = new ReorderableList(property.serializedObject, property, true, true, true, true)
        {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Variants"),
            drawElementCallback = (rect, index, active, focused) =>
            {
                ReorderableList.defaultBehaviours.DrawElement(rect,property.GetArrayElementAtIndex(index),null,readable.IsSelected(index),focused,true,true);
                // EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index));
            },
            onSelectCallback = list => {}
        };
        readable.Select(0);
        return readable;
    }
}