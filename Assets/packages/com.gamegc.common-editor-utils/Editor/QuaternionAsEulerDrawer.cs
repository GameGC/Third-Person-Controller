using GameGC.CommonEditorUtils.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor
{
    [CustomPropertyDrawer(typeof(QuaternionAsEuler))]
    internal class QuaternionAsEulerDrawer : PropertyDrawer
    {
        private Vector3? tempValue;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!tempValue.HasValue)
                tempValue = NormalizeEulerAngles(property.quaternionValue.eulerAngles);

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginProperty(position, label, property);
            tempValue = EditorGUI.Vector3Field(position,label, tempValue.Value);

            EditorGUI.EndProperty();
        
            if (EditorGUI.EndChangeCheck())
            {
                property.quaternionValue = Quaternion.Euler(tempValue.Value);
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Container for all UI elements
            var container = new VisualElement();

            // Initialize tempValue with normalized Euler angles if null
            Vector3 tempValue = NormalizeEulerAngles(property.quaternionValue.eulerAngles);

            // Create a Vector3Field for Euler angles
            var vector3Field = new Vector3Field(property.displayName);
            vector3Field.value = tempValue;

            // Register callback for value changes
            vector3Field.RegisterValueChangedCallback(evt =>
            {
                tempValue = evt.newValue;
                // Apply changes back to quaternion property
                property.quaternionValue = Quaternion.Euler(tempValue);
                property.serializedObject.ApplyModifiedProperties();
            });

            container.Add(vector3Field);

            return container;
        }

        private Vector3 NormalizeEulerAngles(Vector3 angles)
        {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            return angles;
        }

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}
