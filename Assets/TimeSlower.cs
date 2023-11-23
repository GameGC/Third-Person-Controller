using UnityEditor;
using UnityEngine;

public class TimeSlower : EditorWindow
{
    private void OnGUI()
    {
        Time.timeScale = EditorGUILayout.Slider("Time Scale", Time.timeScale, 0, 2);
    }
    
    [MenuItem("Tools/TimeScaler")]
    internal static void SaveAsTemplate() => EditorWindow.GetWindow<TimeSlower>().Show();
}
