using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShellDispancer))]
public class ShellDispancerEditor : Editor
{
    private ShellDispancer target;

    private Vector3 worldPosition;
    private void OnEnable()
    {
        target = base.target as ShellDispancer;
        worldPosition = target.transform.TransformPoint(target.relativePoint);
    }

    private void OnSceneGUI()
    {
        var mid = target.transform.TransformDirection(Vector3.Lerp(target.minVelocity, target.maxVelocity, 0.5f));
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
        
        Handles.ArrowHandleCap(-1,worldPosition,Quaternion.LookRotation(mid),HandleUtility.GetHandleSize(worldPosition)*mid.magnitude,EventType.Repaint);
        
    }
}