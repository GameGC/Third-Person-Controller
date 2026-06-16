using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace GameGC.Collections.Editor
{
    [CustomPropertyDrawer(typeof(SKeyValueList<,>),true)]
    public class SKeyValueListDrawer : PropertyDrawer
    {
        private static Dictionary<string, ReorderableList> _lists = new();
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_lists.TryGetValue(property.propertyPath, out var value))
            {
                try
                {
                    return value.GetHeight();
                }
                catch (Exception e)
                {
                    _lists.Remove(property.propertyPath);
                    return 0;
                }
            }
            return property.FindPropertyRelative("_keys").arraySize * 21f + 20f + 20f + 4f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_lists.TryGetValue(property.propertyPath, out var displayList))
            {
                displayList = new ReorderableList(property.serializedObject, property.FindPropertyRelative("_keys"),true,true,true,true);
                displayList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, label);
                displayList.onChangedCallback += list =>
                {
                    int count = list.count;
                    GetParentProperty(list.serializedProperty).FindPropertyRelative("_values").arraySize = count;
                };
                displayList.onReorderCallbackWithDetails += (list, index, newIndex) =>
                {
                    GetParentProperty(list.serializedProperty).FindPropertyRelative("_values")
                        .MoveArrayElement(index, newIndex);
                };

                var copy = displayList.serializedProperty.Copy();
                displayList.drawElementCallback += (rect, index, _, _) =>  OnDisplayListDrawElementCallback(copy,rect,index);
                displayList.draggable = true;
                _lists.Add(property.propertyPath,displayList);
            }
            displayList.DoList(position);
        }

        protected virtual void OnDisplayListDrawElementCallback(SerializedProperty property,Rect rect, int index)
        {
            var halfRect = rect;
            halfRect.y -= 2;
            halfRect.height -= 2;
            halfRect.width /= 2;
            EditorGUI.PropertyField(halfRect, property.GetArrayElementAtIndex(index), GUIContent.none);
            halfRect.x += halfRect.width;

            EditorGUI.PropertyField(halfRect, GetParentProperty(property)
                .FindPropertyRelative("_values")
                .GetArrayElementAtIndex(index), GUIContent.none);
        }
        private SerializedProperty GetParentProperty(SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            if (path.EndsWith(']'))
                return property.serializedObject.FindProperty(path.Substring(0, path.LastIndexOf('['))
                    .Replace("[", ".Array.data["));

            return property.serializedObject.FindProperty(path.Replace("." + property.name, ""));
        }
    }
}