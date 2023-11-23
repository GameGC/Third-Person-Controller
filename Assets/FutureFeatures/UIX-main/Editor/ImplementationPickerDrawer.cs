using System;
using System.Collections.Generic;
using RedOwl.UIX.Engine;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedOwl.UIX.Editor
{
    [CustomPropertyDrawer(typeof(ImplementationPicker))]
    public class ImplementationPickerDrawer : PropertyDrawer
    {
        private List<Type> _implementations;
        private List<GUIContent> _implementationTitles;
        private int _selectedIndex;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // TODO: Convert to VisualElement
            return null;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_implementations == null || GUILayout.Button("Refresh"))
            {
                _implementations = new List<Type>(TypeExtensions.GetAllTypes(((ImplementationPicker) attribute).type));
                _implementationTitles = new List<GUIContent>(_implementations.Count);
                foreach (var type in _implementations)
                {
                    _implementationTitles.Add(new GUIContent(type.SafeGetName()));
                }
            }
            
            EditorGUILayout.LabelField($"Found {_implementations.Count} implementations");

            _selectedIndex = EditorGUILayout.Popup(new GUIContent("Implementations"), _selectedIndex, _implementationTitles.ToArray());

            if (GUILayout.Button("Create"))
            {
                property.managedReferenceValue = Activator.CreateInstance(_implementations[_selectedIndex]);
            }

            EditorGUILayout.PropertyField(property, true);
        }
    }
}