using System.Collections.Generic;
using CommonEditorUtils.Editor;
using GameGC.CommonEditorUtils.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor
{
    [CustomPropertyDrawer(typeof(EnumArrayAttribute))]
    internal class EnumedArrayDrawer : PropertyDrawer
    {
        /*
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumNames = (attribute as EnumArrayAttribute).EnumType.GetEnumNames();
            var parent = property.GetParentProperty();
            parent.arraySize = enumNames.Length;
            
            Rect labelRect = position;
            labelRect.width /= 3;
            position.x += labelRect.width+1;
            position.width -= labelRect.width-1;

            var path = property.propertyPath;
            int index = int.Parse(path.Substring(path.LastIndexOf('[')+1, path.LastIndexOf(']') - path.LastIndexOf('[')-1));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.LabelField(labelRect, enumNames[index], EditorStyles.popup);
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }*/
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var enumType = (attribute as EnumArrayAttribute).EnumType;
            var enumNames = enumType.GetEnumNames();

            var path = property.propertyPath;
            int index = int.Parse(path.Substring(path.LastIndexOf('[') + 1, path.LastIndexOf(']') - path.LastIndexOf('[') - 1));


            var container = UIElementsExtras.HorizontalContainer;

            // ✅ Use a PopupField with one choice, disabled, to look like a disabled dropdown
            var popup = new PopupField<string>(new List<string> { enumNames[index] }, 0)
            {
                style =
                {
                    flexBasis = new StyleLength(new Length(33, LengthUnit.Percent))
                },
                focusable = false,
                tooltip = enumNames[index]
            };
            popup.SetEnabled(false);

            var field = UIElementsExtras.PropertyField(property, string.Empty);
            ;
            container.Add(popup);
            container.Add(field);

            return container;
        }
    }
}