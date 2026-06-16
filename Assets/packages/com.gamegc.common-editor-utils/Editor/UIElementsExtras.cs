using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommonEditorUtils.Editor
{
    public static class UIElementsExtras
    {
        public static VisualElement HorizontalContainer => new VisualElement
        {
            style = {
                flexDirection = FlexDirection.Row,
                alignItems = Align.Center
            }
        };

        public static VisualElement VerticalContainer => new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                alignItems = Align.FlexStart
            }
        };

        public static Label Label(string text, string tooltip = "")
        {
            return new Label(text)
            {
                style =
                {
                    width = 150,
                    marginRight = 4,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontStyleAndWeight = FontStyle.Normal,
                    whiteSpace = WhiteSpace.NoWrap,
                    overflow = Overflow.Hidden,
                    unityOverflowClipBox = OverflowClipBox.ContentBox
                },
                tooltip = tooltip
            };
        }
        
        public static Label HalfLabel(string text, string tooltip = "")
        {
            return new Label(text)
            {
                style =
                {
                    width = 75,
                    marginRight = 4,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontStyleAndWeight = FontStyle.Normal,
                    whiteSpace = WhiteSpace.NoWrap,
                    unityOverflowClipBox = OverflowClipBox.ContentBox
                },
                tooltip = tooltip
            };
        }

        #region PropertyField

        public static PropertyField PropertyField(SerializedProperty property)
        {
            return PropertyField(property, property.displayName,property.tooltip, true);
        }

        public static PropertyField PropertyField(SerializedProperty property, string label,string tooltip="")
        {
            return PropertyField(property, label,tooltip, true);
        }

        public static PropertyField PropertyField(SerializedProperty property, bool includeChildren)
        {
            return PropertyField(property, null,null, includeChildren);
        }

        private static PropertyField PropertyField(SerializedProperty property, string label,string tooltip="", bool includeChildren = true)
        {
            var field = new PropertyField(property, label)
            {
                style =
                {
                    marginBottom = 2,
                    flexGrow = 1
                },
                tooltip = tooltip
            };

            if (!includeChildren && property.hasVisibleChildren)
            {
                field.BindProperty(property); // Required to bind root
                field.RegisterCallback<GeometryChangedEvent>(_ =>
                {
                    // Remove child fields after geometry is built
                    foreach (var child in field.Children())
                        field.Remove(child);
                });
            }

            return field;
        }
        
        #endregion
    }
}
