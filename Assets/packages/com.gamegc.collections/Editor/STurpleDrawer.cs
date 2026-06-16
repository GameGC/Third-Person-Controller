using UnityEditor;
using UnityEngine;

namespace GameGC.Collections.Editor
{
    [CustomPropertyDrawer(typeof(STurple<,,>))]
    [CustomPropertyDrawer(typeof(STurple<,,,>))]
    [CustomPropertyDrawer(typeof(STurple<,,,,>))]
    public class STurpleDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label,true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => EditorGUI.PropertyField(position,property,label,true);
    }
}