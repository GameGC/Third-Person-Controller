using System;
using ThirdPersonController.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(BaseFeature),true)]
public class DefaultFeatureDrawerVC : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    { 
        property.isExpanded = true;
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

    public static class Styling
    {
        /// <summary>
        /// Style for the override checkbox.
        /// </summary>
        public static readonly GUIStyle smallTickbox;

        /// <summary>
        /// Style for the labels in the toolbar of each effect.
        /// </summary>
        public static readonly GUIStyle miniLabelButton;

        static readonly Color splitterDark;
        static readonly Color splitterLight;

        /// <summary>
        /// Color of UI splitters.
        /// </summary>
        public static Color splitter
        {
            get { return EditorGUIUtility.isProSkin ? splitterDark : splitterLight; }
        }

        static readonly Texture2D paneOptionsIconDark;
        static readonly Texture2D paneOptionsIconLight;

        /// <summary>
        /// Option icon used in effect headers.
        /// </summary>
        public static Texture2D paneOptionsIcon
        {
            get { return EditorGUIUtility.isProSkin ? paneOptionsIconDark : paneOptionsIconLight; }
        }

        /// <summary>
        /// Style for effect header labels.
        /// </summary>
        public static readonly GUIStyle headerLabel;

        static readonly Color headerBackgroundDark;
        static readonly Color headerBackgroundLight;

        /// <summary>
        /// Color of effect header backgrounds.
        /// </summary>
        public static Color headerBackground
        {
            get { return EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight; }
        }

        /// <summary>
        /// Style for the trackball labels.
        /// </summary>
        public static readonly GUIStyle wheelLabel;

        /// <summary>
        /// Style for the trackball cursors.
        /// </summary>
        public static readonly GUIStyle wheelThumb;

        /// <summary>
        /// Size of the trackball cursors.
        /// </summary>
        public static readonly Vector2 wheelThumbSize;

        /// <summary>
        /// Style for the curve editor position info.
        /// </summary>
        public static readonly GUIStyle preLabel;

        static Styling()
        {
            smallTickbox = new GUIStyle("ShurikenToggle");

            miniLabelButton = new GUIStyle(EditorStyles.miniLabel);
            miniLabelButton.normal = new GUIStyleState
            {
             //   background = RuntimeUtilities.transparentTexture,
                scaledBackgrounds = null,
                textColor = Color.grey
            };
            var activeState = new GUIStyleState
            {
              //  background = RuntimeUtilities.transparentTexture,
                scaledBackgrounds = null,
                textColor = Color.white
            };
            miniLabelButton.active = activeState;
            miniLabelButton.onNormal = activeState;
            miniLabelButton.onActive = activeState;

            splitterDark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
            splitterLight = new Color(0.6f, 0.6f, 0.6f, 1.333f);

            headerBackgroundDark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
            headerBackgroundLight = new Color(1f, 1f, 1f, 0.2f);

            paneOptionsIconDark = (Texture2D) EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            paneOptionsIconLight = (Texture2D) EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");

            headerLabel = new GUIStyle(EditorStyles.miniLabel);

            wheelThumb = new GUIStyle("ColorPicker2DThumb");

            wheelThumbSize = new Vector2(
                !Mathf.Approximately(wheelThumb.fixedWidth, 0f) ? wheelThumb.fixedWidth : wheelThumb.padding.horizontal,
                !Mathf.Approximately(wheelThumb.fixedHeight, 0f) ? wheelThumb.fixedHeight : wheelThumb.padding.vertical
            );

            wheelLabel = new GUIStyle(EditorStyles.miniLabel);

            preLabel = new GUIStyle("ShurikenLabel");
        }
    }

    internal static bool DrawHeader(string title, SerializedProperty group, SerializedProperty activeField, Object target, Action resetAction, Action removeAction)
        {

            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 32f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var toggleRect = backgroundRect;
            toggleRect.x += 16f;
            toggleRect.y += 2f;
            toggleRect.width = 13f;
            toggleRect.height = 13f;

            var menuIcon = Styling.paneOptionsIcon;
            var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y, menuIcon.width, menuIcon.height);


            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, Styling.headerBackground);

            // Title
            using (new EditorGUI.DisabledScope(!activeField.boolValue))
                EditorGUI.LabelField(labelRect, EditorGUIUtility.TrTextContent(title), EditorStyles.boldLabel);

            // foldout
            group.serializedObject.Update();
            group.isExpanded = GUI.Toggle(foldoutRect, group.isExpanded, GUIContent.none, EditorStyles.foldout);
            group.serializedObject.ApplyModifiedProperties();

            // Active checkbox
            activeField.serializedObject.Update();
            activeField.boolValue = GUI.Toggle(toggleRect, activeField.boolValue, GUIContent.none, Styling.smallTickbox);
            activeField.serializedObject.ApplyModifiedProperties();

            // Dropdown menu icon
            GUI.DrawTexture(menuRect, menuIcon);

            // Handle events
            var e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                if (menuRect.Contains(e.mousePosition))
                {
                    //ShowHeaderContextMenu(new Vector2(menuRect.x, menuRect.yMax), target, resetAction, removeAction);
                    e.Use();
                }
                else if (labelRect.Contains(e.mousePosition))
                {
                    if (e.button == 0)
                        group.isExpanded = !group.isExpanded;
                    else
                   //     ShowHeaderContextMenu(e.mousePosition, target, resetAction, removeAction);

                    e.Use();
                }
            }

            return group.isExpanded;
        }
}