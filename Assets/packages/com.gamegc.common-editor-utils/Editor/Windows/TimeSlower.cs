using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor.Windows
{
    internal class TimeSlower : EditorWindow
    {
        [MenuItem("Tools/TimeScaler")]
        public static void ShowWindow()
        {
            var window = GetWindow<TimeSlower>();
            window.titleContent = new GUIContent("Time Scaler");
        }

        public void CreateGUI()
        {
            var slider = new Slider("Time Scale", 0f, 2f)
            {
                value = Time.timeScale
            };

            slider.RegisterValueChangedCallback(evt =>
            {
                Time.timeScale = evt.newValue;
            });
            rootVisualElement.Add(slider);
        }
    }
}