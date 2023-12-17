using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumArrayAttribute))]
internal class EnumedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var enumNames = (attribute as EnumArrayAttribute).EnumType.GetEnumNames();
        var parent = property.GetParentProperty();
        parent.arraySize = enumNames.Length;
            
        Rect labelRect = position;
        labelRect.width /= 3;
        position.x += labelRect.width+1;
        position.width -= labelRect.width-1;

        var path = property.propertyPath;
        int index = int.Parse(path.Substring(path.LastIndexOf('[')+1, path.LastIndexOf(']') - path.LastIndexOf('[')-1));

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.LabelField(labelRect, enumNames[index], EditorStyles.popup);
        EditorGUI.EndDisabledGroup();
            
        EditorGUI.PropertyField(position, property, GUIContent.none);
    }
}