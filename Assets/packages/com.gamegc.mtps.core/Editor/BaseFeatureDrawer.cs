using System.Linq;
using GameGC.CommonEditorUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace MTPS.Core.Editor
{
    [CustomPropertyDrawer(typeof(BaseFeature), true)]
    public class BaseFeatureDrawer : PropertyDrawer
    {
        private static BaseFeatureDrawerVC fallback = new BaseFeatureDrawerVC();
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return fallback.GetPropertyHeight(property, label);
            
            if (property.CountChildProperties() < 3) return EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                fallback.OnGUI(position, property, label);
                return;
            }

            if (property.FindPropertyRelative("path") == null) return;
            property.FindPropertyRelative("path").stringValue = property.propertyPath;
            label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));

            //Debug.Log(label.text+" "+property.CountChildProperties());
            int childsCount = property.CountChildProperties();
            if (childsCount < 3)
            {
                if (childsCount > 1)
                {
                    property.NextVisible(true);
                    GCUtils.DrawSmallPropertyOneLine2Labels(property, position, label, false);
                }
                else
                {
                    EditorGUI.LabelField(position, label);
                }

                return;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        private string GetPropertyTypeName(SerializedProperty property)
        {
            string actionName = property.managedReferenceFullTypename.Split(" ").Last();

            var split = actionName.Split('.');
            if (split.Length > 2) actionName = split[^2] + '.' + split[^1];

            return actionName;
        }
    }
}