using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShellDispancer : MonoBehaviour
{
    public Rigidbody shell;
    public float shellLifeTime;

    public Vector3 relativePoint;
    
    public Vector3 minVelocity;
    public Vector3 maxVelocity;

    public Vector3 minTorque;
    public Vector3 maxTorque;
    public void OnShoot()
    {
        var shell = Instantiate(this.shell, transform.TransformPoint(relativePoint), Quaternion.identity, null);
        shell.AddForce(transform.TransformDirection(GetRandomVector(minVelocity,maxVelocity)),ForceMode.Impulse);
        shell.AddTorque(GetRandomVector(minTorque,maxTorque),ForceMode.Force);
        Destroy(shell.gameObject,shellLifeTime);
    }

    private Vector3 GetRandomVector(Vector3 min, Vector3 max)
    {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    }
}

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