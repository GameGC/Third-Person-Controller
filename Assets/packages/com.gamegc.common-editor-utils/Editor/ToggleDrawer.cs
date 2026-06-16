using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace GameGC.CommonEditorUtils.Editor
{
    public class ToggleDrawer : ScriptableSingleton<ToggleDrawer>
    {
        [SerializeField] public Texture2D toogleDot;
        [SerializeField] public Texture toogleDotBlue;
        [SerializeField] public Texture2D whiteToogleDot;
    
        public static bool CustomToggle(Rect rect, bool value, GUIContent label)
        {
            Rect position = new Rect(rect)
            {
                width = EditorGUIUtility.labelWidth + 6f
            };
            GUI.Label(position, label);
            rect.x += EditorGUIUtility.labelWidth + 8f;
            return instance.FeatureToggleInstance(rect, value, label);
        }
        
        public static bool CustomToggle(Rect rect, bool value, GUIContent label, GUIStyle labelStyle = null)
        {
            labelStyle ??= EditorStyles.label;
            Rect position = new Rect(rect)
            {
                width = EditorGUIUtility.labelWidth + 6f
            };
            GUI.Label(position, label,labelStyle);
            rect.x += EditorGUIUtility.labelWidth + 8f;
            return instance.FeatureToggleInstance(rect, value, label);
        }
        
        public static bool CustomToggle(Rect rect, bool value, [NotNull] GUIContent label, bool withoutLabel = false)
        {
            if (withoutLabel)
            {
                Rect position = new Rect(rect)
                {
                    width = EditorGUIUtility.labelWidth + 6f
                };
                GUI.Label(position, label);
            }
            rect.x += EditorGUIUtility.labelWidth + 8f;
            return instance.FeatureToggleInstance(rect, value, label);
        }

        public static bool NeedRepaint(GUIContent content) => instance.Repaint(content);

        private Dictionary<GUIContent, double> times = new Dictionary<GUIContent, double>();
        private bool FeatureToggleInstance(Rect rect, bool value, GUIContent label)
        {
            rect.width = 34f;
            rect.height = 16f;
            if (GUI.Button(rect, GUIContent.none, value?ActiveToggleStyle:ToggleStyle))
            {
                value = !value;
                if (times.ContainsKey(label))
                {
                    times[label] = EditorApplication.timeSinceStartup;
                }
                else times.Add(label,EditorApplication.timeSinceStartup);
            }

            times.TryGetValue(label, out var lastRepaintTime);
            float anim = (float) (EditorApplication.timeSinceStartup - lastRepaintTime) * 10f;
            float currentValue = value ? rect.x : rect.x + rect.width * 0.5f;
            rect.x = Mathf.Lerp(currentValue,value ? rect.x + rect.width * 0.5f : rect.x, anim);
            rect.width = rect.height;
        
            GUI.DrawTexture(rect, whiteToogleDot);
            return value;
        }

        public bool Repaint(GUIContent content)
        {
            times.TryGetValue(content, out var time);
            return (EditorApplication.timeSinceStartup - time) * 10f < 1;
        }

        private GUIStyle _toggleStyle;
        private GUIStyle ToggleStyle
        {
            get
            {
                if (_toggleStyle == null)
                    _toggleStyle = new GUIStyle()
                    {
                        imagePosition = ImagePosition.TextOnly,
                        alignment = TextAnchor.MiddleRight,
                        clipping = TextClipping.Overflow,
                        wordWrap = true,
                        stretchWidth = false,
                        richText = false,
                        name = "ctoggle",
                        border = new RectOffset(8, 8, 4, 4),
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 45, 0, 0),
                        overflow = new RectOffset(0, 0, 0, 0),
                        normal = new GUIStyleState()
                        {
                            background = (Texture2D)toogleDot,
                            textColor = Color.white
                        },
                        hover = new GUIStyleState()
                        {
                            textColor = new Color(0,0.9263f,1,1),
                        },
                        active = new GUIStyleState()
                        {
                            textColor = new Color(1,0.8901961f,0,1)
                        },
                        focused = new GUIStyleState()
                        {
                            textColor = new Color(0.9254902f,0.08627451f,0.3686275f,1)
                        },
                        onNormal = new GUIStyleState()
                        {
                            background = (Texture2D) toogleDotBlue,
                            textColor = Color.white
                        },
                        onHover = new GUIStyleState()
                        {
                            background = (Texture2D)toogleDotBlue,
                            textColor = new Color(0.4078432f,1,1,1)
                        },
                    };
                return _toggleStyle;
            }
        }

        private GUIStyle _activeToggleStyle;
        private GUIStyle ActiveToggleStyle
        {
            get
            {
                if (_activeToggleStyle == null)
                {
                    _activeToggleStyle = new GUIStyle(ToggleStyle)
                    {
                        normal =
                        {
                            background = _toggleStyle.onNormal.background
                        }
                    };
                }

                return _activeToggleStyle;
            }
        }
    }
}