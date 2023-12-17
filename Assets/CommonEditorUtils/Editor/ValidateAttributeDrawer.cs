using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ValidateBaseTypeAttribute))]
internal class ValidateAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attribute = base.attribute as ValidateBaseTypeAttribute;
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label);
        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
            
            if(!property.objectReferenceValue) return;
            int index = Array.IndexOf(attribute.SupportedBaseTypes, property.objectReferenceValue.GetType());
            if (index < 0)
            {
                string supportedTypeList = "These types are supported: ";
                for (var i = 0; i < attribute.SupportedBaseTypes.Length; i++)
                {
                    var type = attribute.SupportedBaseTypes[i];
                    supportedTypeList += type.Name + (i < attribute.SupportedBaseTypes.Length -1 ?", ":"");
                }
                
                EditorUtility.DisplayDialog("This type not supported",supportedTypeList,"ok");
                EditorApplication.update += Reset;
            }
        }
    }

    private void Reset()
    {
        Undo.PerformUndo();
        EditorApplication.update -= Reset;
    }
}