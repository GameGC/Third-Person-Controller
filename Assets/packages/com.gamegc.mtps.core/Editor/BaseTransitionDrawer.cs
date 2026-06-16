using System.Linq;
using GameGC.CommonEditorUtils.Editor;
using MTPS.Core.CodeStateMachine;
using UnityEditor;
using UnityEngine;

namespace MTPS.Core.Editor
{
    [CustomPropertyDrawer(typeof(BaseStateTransition), true)]
    public class BaseTransitionDrawer : PropertyDrawer
    {
        public static bool hideDestinationState;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            if (!property.IsParentExpanded()) return height;

            int propertyCount = property.CountChildProperties();
            ;

            if (hideDestinationState)
            {
                if (propertyCount < 4) return EditorGUIUtility.singleLineHeight;
                if (property.isExpanded) height -= EditorGUIUtility.singleLineHeight;
            }
            else
            {
                if (propertyCount < 3) return EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //if (!property.IsParentExpanded()) return;
            label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));

            string initPath = property.propertyPath;
            int propertyCount = property.CountChildProperties();

            if (!hideDestinationState)
            {
                if (propertyCount < 3)
                {
                    //EditorGUI.LabelField(property,label);
                    DrawSmallProperty(property, position, label, initPath, false);
                }
                else
                    EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                Rect temp = position;
                temp.height = EditorGUIUtility.singleLineHeight;
                position.height -= temp.height;
                position.y += temp.height;

                if (propertyCount < 4)
                {
                    if (propertyCount < 3)
                        EditorGUI.LabelField(temp, label);
                    else
                        DrawSmallProperty(property, temp, label, initPath);
                    return;
                }

                EditorGUI.PropertyField(temp, property, label, false);
                if (property.isExpanded)
                {
                    string basePath = property.propertyPath;

                    property.NextVisible(true);
                    if (property.name != "_transitionName" && property.name != "_transitionIndex")
                    {
                        temp = position;
                        temp.height = EditorGUI.GetPropertyHeight(property);
                        EditorGUI.PropertyField(temp, property, false);
                        position.height -= temp.height;
                        position.y += temp.height;
                    }

                    //pCount--;
                    while (property.NextVisible(false) && property.propertyPath.StartsWith(basePath))
                    {
                        if (property.name != "_transitionName" && property.name != "_transitionIndex")
                        {
                            temp = position;
                            temp.height = EditorGUI.GetPropertyHeight(property);
                            EditorGUI.PropertyField(temp, property, false);
                            position.height -= temp.height;
                            position.y += temp.height;
                        }
                    }
                }
            }
        }

        private void DrawSmallProperty(SerializedProperty property, Rect temp, GUIContent label, string initPath,
            bool hideDestination = true)
        {
            property.NextVisible(true);
            if (!hideDestinationState)
            {
                GCUtils.DrawSmallPropertyOneLine2Labels(property, temp, label);
                return;
            }

            while (property.NextVisible(false) && property.propertyPath.StartsWith(initPath))
            {
                if (property.name is not ("_transitionName" or "_transitionIndex"))
                {
                    GCUtils.DrawSmallPropertyOneLine2Labels(property, temp, label);
                    return;
                }
            }
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