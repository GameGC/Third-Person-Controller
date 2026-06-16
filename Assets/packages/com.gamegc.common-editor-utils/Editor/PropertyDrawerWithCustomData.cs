#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor
{
    public abstract class PropertyDrawerWithCustomData<T> : PropertyDrawer
    {
        private readonly Dictionary<string, T> _collection = new Dictionary<string, T>();

        private bool _callSecondOnEnable;

        protected void OnEnable(SerializedProperty property, GUIContent label, T customData) { }
        protected virtual void OnEnable(Rect position, SerializedProperty property, GUIContent label, T customData) { }

        protected virtual float GetPropertyHeight(SerializedProperty property, GUIContent label, T customData) => 
            base.GetPropertyHeight(property, label);

        protected abstract void OnGUI(Rect position, SerializedProperty property, GUIContent label, T customData);

        protected virtual VisualElement CreatePropertyGUI(SerializedProperty property, T customData)
        {
            return new IMGUIContainer(() =>
            {
                var rect = EditorGUILayout.GetControlRect();
                OnGUI(rect, property, new GUIContent(property.displayName ?? property.name), customData);
            });
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!_collection.TryGetValue(property.propertyPath, out var customData))
            {
                customData = Activator.CreateInstance<T>();
                _collection.Add(property.propertyPath, customData);

                using (var copy = GetCopy(property))
                {
                    OnEnable(copy, label, customData);
                }

                _callSecondOnEnable = true;
            }

            return GetPropertyHeight(property, label, customData);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_collection.TryGetValue(property.propertyPath, out var customData))
            {
                customData = Activator.CreateInstance<T>();
                _collection.Add(property.propertyPath, customData);
            }

            if (Event.current.type == EventType.Repaint)
            {
                if (_callSecondOnEnable)
                {
                    _callSecondOnEnable = false;
                    using (var copy = GetCopy(property))
                    {
                        OnEnable(position, copy, label, customData);
                    }
                }
            }
            if(_callSecondOnEnable) return;

            OnGUI(position, property, label, customData);
        }

        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    if (!_collection.TryGetValue(property.propertyPath, out var customData))
        //    {
        //        customData = Activator.CreateInstance<T>();
        //        _collection.Add(property.propertyPath, customData);
//
        //        using (var copy = GetCopy(property))
        //            OnEnable(copy, new GUIContent(property.displayName ?? property.name), customData);
//
        //        _callSecondOnEnable = true;
        //    }
//
        //    return CreatePropertyGUI(property, customData);
        //}

        protected static SerializedProperty GetCopy(SerializedProperty property) => property.serializedObject.FindProperty(property.propertyPath);
    }
}

#endif
