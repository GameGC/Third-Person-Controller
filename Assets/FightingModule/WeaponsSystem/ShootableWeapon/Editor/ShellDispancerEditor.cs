using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShellDispancer))]
public class ShellDispancerEditor : Editor
{
    private ShellDispancer target;

    private Vector3 worldPosition;
    private void OnEnable()
    {
        if(base.target == null) return;
        target = base.target as ShellDispancer;
        worldPosition = target.transform.TransformPoint(target.relativePoint);
    }
    
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        GUI.enabled = true;

        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shell"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shellLifeTime"));
        }
        GUILayout.Space(9f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relativePoint"));
        
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minVelocity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxVelocity"));
        }
            
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minTorque"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxTorque"));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        var mid = target.transform.TransformDirection(Vector3.Lerp(target.minVelocity, target.maxVelocity, 0.5f));
        if (!Application.isPlaying)
        {
            if (EditorUtility.IsDirty(target.transform))
            {
                worldPosition = target.transform.TransformPoint(target.relativePoint);
                EditorUtility.ClearDirty(target.transform);
            }

            Handles.color = Color.magenta;

            EditorGUI.BeginChangeCheck();
            var newPos = Handles.PositionHandle(worldPosition, Quaternion.LookRotation(mid));
            if (EditorGUI.EndChangeCheck())
            {
                worldPosition = newPos;
                target.relativePoint = target.transform.InverseTransformPoint(newPos);
            }
        }
        else
        {
            worldPosition = target.transform.TransformPoint(target.relativePoint);
            Handles.PositionHandle(worldPosition, Quaternion.LookRotation(mid));
        }

        var dirMin = target.transform.TransformDirection(target.minVelocity);
        Handles.color = Color.green;
        Handles.ArrowHandleCap(-1, worldPosition, Quaternion.LookRotation(dirMin),
            HandleUtility.GetHandleSize(worldPosition) * dirMin.magnitude, EventType.Repaint);

        var dirMax = target.transform.TransformDirection(target.maxVelocity);
        Handles.color = Color.red;
        Handles.ArrowHandleCap(-1, worldPosition, Quaternion.LookRotation(dirMax),
            HandleUtility.GetHandleSize(worldPosition) * dirMax.magnitude, EventType.Repaint);
    }
}