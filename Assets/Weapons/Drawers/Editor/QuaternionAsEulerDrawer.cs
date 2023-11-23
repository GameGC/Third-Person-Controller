using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuaternionAsEuler))]
public class QuaternionAsEulerDrawer : PropertyDrawer
{
    private Vector3? tempValue;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!tempValue.HasValue)
            tempValue = property.quaternionValue.ToEuler() * 57.29578f;

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
}
