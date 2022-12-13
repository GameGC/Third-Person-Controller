using ThirdPersonController.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseFeature),true)]
public class DefaultProprtyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    { property.isExpanded = true;
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        if (string.IsNullOrWhiteSpace(property.FindPropertyRelative("path").stringValue))
        {
            rect.height = 18f;

            if (GUI.Button(rect, "Create"))
            {
                property.FindPropertyRelative("path").stringValue = property.propertyPath;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            rect.y += 18f;
        }

        SerializedProperty endProp = property.GetEndProperty();

        property.NextVisible(true);

        int i = 0;
        while (endProp.propertyPath != property.propertyPath)
        {
            rect.y += i > 0 ? GetDefaultSpaceBetweenElements(rect.height) : 0;
            rect.height = EditorGUI.GetPropertyHeight(property);

            EditorGUI.PropertyField(rect, property, false);

            ++i;
            property.NextVisible(false);
        }
    }
    
    private float GetDefaultSpaceBetweenElements(float previousHeight = 0)
    {
        return (previousHeight>0?previousHeight:EditorGUIUtility.singleLineHeight) + EditorGUIUtility.standardVerticalSpacing;
    }
}