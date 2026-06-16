using UnityEditor;
using UnityEngine;

namespace GameGC.Collections.Editor
{
    [CustomPropertyDrawer(typeof(SNullable<>))]
    public class SNullableDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"), label,true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("value");
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position,valueProperty , label, true);
            if (EditorGUI.EndChangeCheck())
            {
                property.FindPropertyRelative("<HasValue>k__BackingField").boolValue = true;
            }

            valueProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}