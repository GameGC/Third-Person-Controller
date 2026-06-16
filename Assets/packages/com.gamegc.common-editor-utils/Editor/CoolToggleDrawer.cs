using GameGC.CommonEditorUtils.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor
{
    [CustomPropertyDrawer(typeof(CoolToggle))]
    public class CoolToggleDrawer : PropertyDrawer
    {
        private new CoolToggle attribute => (CoolToggle)base.attribute;

        // Override the CreatePropertyGUI method for UIElements
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Main container for the property drawer UI
            var container = new VisualElement();

            // Use custom title if provided, otherwise use property's display name
            string labelText = string.IsNullOrEmpty(attribute.Title) 
                ? property.displayName 
                : attribute.Title;

            // Create the custom toggle visual element (includes label, track, knob, and hidden toggle)
            var toggleVisual = ToggleDrawerNew.CreateCustomToggle(property.propertyPath,labelText, property.boolValue);

            // Extract the hidden Toggle component from the visual tree
            var hiddenToggle = toggleVisual.Q<Toggle>();
            if (hiddenToggle != null)
            {
                hiddenToggle.value = property.boolValue;
                hiddenToggle.RegisterValueChangedCallback(evt =>
                {
                    property.boolValue = evt.newValue;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            // Optional: Add indentation if inside nested property fields
            if (EditorGUI.indentLevel > 0)
            {
                var indentContainer = new VisualElement();
                indentContainer.style.paddingLeft = 15 * EditorGUI.indentLevel;
                indentContainer.Add(toggleVisual);
                container.Add(indentContainer);
            }
            else
            {
                container.Add(toggleVisual);
            }

            return container;
        }

        // Keep the IMGUI version for backward compatibility
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIContent title = string.IsNullOrEmpty(attribute.Title)
                ? label
                : EditorGUIUtility.TrTextContent(property.displayName);
            position.x += 15 * EditorGUI.indentLevel;
            property.boolValue = ToggleDrawer.CustomToggle(position, property.boolValue, title);
        }
        
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }
    }
}