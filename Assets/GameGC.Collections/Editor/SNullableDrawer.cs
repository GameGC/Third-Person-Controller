using System;
using System.Linq;
using System.Reflection;
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
            
           //r rect = new Rect((position.x+ position.width) - 18, position.y, 18, Mathf.Min(position.height,18));
           // (GUI.Button(rect,"X"))
           //
           //  Type type = null;
           //  foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
           //  {
           //      type = assembly.GetType(valueProperty.type);
           //      if(type!=null) break;
           //  }
           //  
           //  
           // //var field = type.GetField(valueProperty.propertyPath,BindingFlags.Instance);
           // //field.SetValue(valueProperty.serializedObject.targetObject,null);

           // //var member = type.GetMember(valueProperty.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.')));
           // //member.GetValue()
           // //
           // //property.serializedObject.ApplyModifiedProperties();
           //
           //
            
            EditorGUI.PropertyField(position,valueProperty , label, true);

           
        }
    }
}