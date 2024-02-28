using MTPS.Shooter.WeaponsSystem.ShootableWeapon.Extensions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShellDispancer))]
public class ShellDispancerEditor : BaseWeaponExtensionEditor
{
    private new ShellDispancer target;

    private Vector3 _worldPosition;
    private void OnEnable()
    {
        if(base.target == null) return;
        target = base.target as ShellDispancer;
        _worldPosition = target.transform.TransformPoint(target.relativePoint);
    }
    
    public override void OnInspectorGUI()
    {
        DrawScriptHeader();

        using (new GUILayout.VerticalScope(box))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shell"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shellLifeTime"));
        }
        GUILayout.Space(9f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relativePoint"));
        
        using (new GUILayout.VerticalScope(box))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minVelocity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxVelocity"));
        }
            
        using (new GUILayout.VerticalScope(box))
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
                _worldPosition = target.transform.TransformPoint(target.relativePoint);
                EditorUtility.ClearDirty(target.transform);
            }

            Handles.color = Color.magenta;

            EditorGUI.BeginChangeCheck();
            var newPos = Handles.PositionHandle(_worldPosition, Quaternion.LookRotation(mid));
            if (EditorGUI.EndChangeCheck())
            {
                _worldPosition = newPos;
                target.relativePoint = target.transform.InverseTransformPoint(newPos);
            }
        }
        else
        {
            _worldPosition = target.transform.TransformPoint(target.relativePoint);
            Handles.PositionHandle(_worldPosition, Quaternion.LookRotation(mid));
        }

        var dirMin = target.transform.TransformDirection(target.minVelocity);
        Handles.color = Color.green;
        Handles.ArrowHandleCap(-1, _worldPosition, Quaternion.LookRotation(dirMin),
            HandleUtility.GetHandleSize(_worldPosition) * dirMin.magnitude, EventType.Repaint);

        var dirMax = target.transform.TransformDirection(target.maxVelocity);
        Handles.color = Color.red;
        Handles.ArrowHandleCap(-1, _worldPosition, Quaternion.LookRotation(dirMax),
            HandleUtility.GetHandleSize(_worldPosition) * dirMax.magnitude, EventType.Repaint);
    }
}