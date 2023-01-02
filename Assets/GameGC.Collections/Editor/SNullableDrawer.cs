using UnityEditor;
using UnityEngine;

namespace GameGC.Collections.Editor
{
    [CustomPropertyDrawer(typeof(SNullable<>))]
    public class SNullableDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"), label,true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), label, true);
    }
}