using UnityEditor;
using UnityEngine;

namespace GameGC.Collections.Editor
{
    [CustomPropertyDrawer(typeof(SKeyValuePair<,>))]
    public class SKeyValuePairDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var keyProp = property.FindPropertyRelative("Key");
            var valueProp = property.FindPropertyRelative("Value");
            return Mathf.Max(EditorGUI.GetPropertyHeight(keyProp), EditorGUI.GetPropertyHeight(valueProp));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool hasLabel = !property.propertyPath.EndsWith(']');
            
            var keyProp = property.FindPropertyRelative("Key");
            var valueProp = property.FindPropertyRelative("Value");

            if (hasLabel)
            {
                position.width /= 4;
                EditorGUI.PrefixLabel(position,label);
                position.x += position.width;
                position.width *= 3;
            }

            var keyPos = position;
            keyPos.width /= 2;
            keyPos.height = EditorGUI.GetPropertyHeight(keyProp);
            var valuePos = keyPos;
            valuePos.x += valuePos.width;
            valuePos.height = EditorGUI.GetPropertyHeight(valueProp);
                
            EditorGUI.PropertyField(keyPos,keyProp,GUIContent.none);
            EditorGUI.PropertyField(valuePos,valueProp,GUIContent.none);
        }
    }
}
