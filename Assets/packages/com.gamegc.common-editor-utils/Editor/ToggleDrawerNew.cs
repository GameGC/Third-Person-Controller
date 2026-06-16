using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor
{
    public class ToggleDrawerNew : ScriptableSingleton<ToggleDrawerNew>
    {
        private Dictionary<string, double> animationTimes = new Dictionary<string, double>();
        private Dictionary<string, bool> toggleStates = new Dictionary<string, bool>();
        private Dictionary<string, IVisualElementScheduledItem> runningAnimations = new Dictionary<string, IVisualElementScheduledItem>();

        // Creates a custom toggle in a UIElements-based editor
        public static VisualElement CreateCustomToggle(string path,string label, bool initialValue)
        {
            return instance.CreateToggleInstance(path,label, initialValue);
        }

        // Creates a custom toggle with a specific style
        public static VisualElement CreateCustomToggle(string path,string label, bool initialValue, StyleSheet customStyle = null)
        {
            return instance.CreateToggleInstance(path,label, initialValue, customStyle);
        }

        // Checks if the toggle needs to be repainted (for animation)
        public static bool NeedRepaint(string path)
        {
            return instance.Repaint(path);
        }
        private VisualElement CreateToggleInstance(string path,string label, bool initialValue, StyleSheet customStyle = null)
        {
            // Main container
            var container = new VisualElement
            {
                name = path,
                style = {flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4}
            };

            // Label
            var toggleLabel = new Label(label)
            {
                style =
                {
                    minWidth = 150,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontStyleAndWeight = FontStyle.Normal,
                    fontSize = 12,
                    color = EditorGUIUtility.isProSkin 
                        ? new Color(0.76f, 0.76f, 0.76f)  // light gray for dark theme
                        : Color.black                    // black for light theme
                }
            };
            container.Add(toggleLabel);

            // Toggle track (background)
            var toggleTrack = new VisualElement
            {
                name = "ToggleTrack",
                style =
                {
                    width = 34,
                    height = 16,
                    backgroundColor = initialValue ? new Color(0f, 0.5f, 1f) : new Color(0.3f, 0.3f, 0.3f),
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    marginLeft = 8,
                    position = Position.Relative
                }
            };
            container.Add(toggleTrack);

            // Toggle knob (circle)
            var toggleKnob = new VisualElement
            {
                name = "ToggleKnob",
                style =
                {
                    width = 16,
                    height = 16,
                    backgroundColor = Color.white,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    position = Position.Absolute,
                    left = initialValue ? 18 : 0,
                    top = 0
                }
            };
            toggleTrack.Add(toggleKnob);

            // Create hidden Toggle for binding
            var hiddenToggle = new Toggle {value = initialValue};
            hiddenToggle.style.display = DisplayStyle.None;
            container.Add(hiddenToggle);

            // Apply custom style if any
            if (customStyle != null)
            {
                container.styleSheets.Add(customStyle);
            }

            // Store toggle state & animation time
            toggleStates[path] = initialValue;
            animationTimes[path] = EditorApplication.timeSinceStartup;

            // When clicked, toggle the value
            toggleTrack.RegisterCallback<ClickEvent>(evt => { hiddenToggle.value = !hiddenToggle.value; });

            // Listen to toggle value changes
            hiddenToggle.RegisterValueChangedCallback(evt =>
            {
                bool newValue = evt.newValue;

                toggleStates[path] = newValue;
                toggleTrack.style.backgroundColor = newValue ? new Color(0f, 0.5f, 1f) : new Color(0.3f, 0.3f, 0.3f);
                animationTimes[path] = EditorApplication.timeSinceStartup;

                AnimateToggle(path, container, toggleKnob, newValue);
            });

            container.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                toggleStates.Remove(path);
                animationTimes.Remove(path);

                if (runningAnimations.TryGetValue(path, out var scheduled))
                {
                    scheduled.Pause();
                    runningAnimations.Remove(path);
                }
            });


            return container;
        }
        private void AnimateToggle(string path, VisualElement container, VisualElement knob, bool targetValue)
        {
            // Cancel previous animation if it exists
            if (runningAnimations.TryGetValue(path, out var scheduled))
            {
                scheduled.Pause();
                runningAnimations.Remove(path);
            }

            animationTimes[path] = EditorApplication.timeSinceStartup;

            const float animationDuration = 0.15f; // duration of the animation in seconds

            float startPos = targetValue ? 0 : 18;
            float endPos = targetValue ? 18 : 0;

            IVisualElementScheduledItem newSchedule = null;
            newSchedule = container.schedule.Execute(() =>
            {
                if (!animationTimes.TryGetValue(path, out var startTime))
                {
                    runningAnimations.Remove(path);
                    newSchedule.Pause();
                    return;
                }

                float t = (float)((EditorApplication.timeSinceStartup - startTime) / animationDuration);
                if (t >= 1f)
                {
                    knob.style.left = endPos;
                    runningAnimations.Remove(path);
                    animationTimes.Remove(path);
                    newSchedule.Pause();
                    return;
                }

                knob.style.left = Mathf.Lerp(startPos, endPos, t);

            }).Every(10);

            runningAnimations[path] = newSchedule;
        }

        private bool Repaint(string path)
        {
            animationTimes.TryGetValue(path, out var time);
            return (EditorApplication.timeSinceStartup - time) * 10f < 1;
        }
    }
}