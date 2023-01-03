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
            TryDrawLabel(ref position, property, label);
            
            OnKeyGUI(property, position);
            OnValueGUI(property,position);
        }


        public void TryDrawLabel(ref Rect position, SerializedProperty property, GUIContent label)
        {
            bool hasLabel = !property.propertyPath.EndsWith(']');
            if (!hasLabel) return;
            
            position.width /= 4;
            EditorGUI.PrefixLabel(position,label);
            position.x += position.width;
            position.width *= 3;
        }
        
        public void OnKeyGUI(SerializedProperty property,Rect sourceRect)
        {
            var keyProp = property.FindPropertyRelative("Key");
            
            var keyPos = sourceRect;
            keyPos.width /= 2;
            keyPos.height = EditorGUI.GetPropertyHeight(keyProp);
            
            EditorGUI.PropertyField(keyPos,keyProp,GUIContent.none);
        }
        
        public void OnValueGUI(SerializedProperty property,Rect sourceRect)
        {
            var valueProp = property.FindPropertyRelative("Value");
            
            var valuePos = sourceRect;
            valuePos.width /= 2;
            valuePos.x += valuePos.width;
            valuePos.height = EditorGUI.GetPropertyHeight(valueProp);
            
            
            EditorGUI.PropertyField(valuePos,valueProp,GUIContent.none);
        }
    }
}
