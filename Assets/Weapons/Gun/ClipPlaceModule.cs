using UnityEditor;
using UnityEngine;

public class ClipPlaceModule : BaseWeaponExtension
{
    public Transform clipElement;
    public Vector3 localPositionInLeftHand;
    public Quaternion localRotationInLeftHand;

    private Pose _initialPosition;


    private void Awake()
    {
        clipElement.GetLocalPositionAndRotation(out _initialPosition.position,out _initialPosition.rotation);
    }

    public void AttachToLeftHand()
    {
        clipElement.SetParent(transform.root.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftHand));
        clipElement.SetLocalPositionAndRotation(localPositionInLeftHand,localRotationInLeftHand);
    }
    
    public void AttachToRightHand()
    {
        clipElement.SetParent(transform);
        clipElement.SetLocalPositionAndRotation(_initialPosition.position,_initialPosition.rotation);
    }

#if UNITY_EDITOR
    [ContextMenu("CopyVariables")]
    public void CopyVariables() => clipElement.GetLocalPositionAndRotation(out localPositionInLeftHand,out localRotationInLeftHand);

    [ContextMenu("PasteVariables")]
    public void PasteVariables() => clipElement.SetLocalPositionAndRotation(localPositionInLeftHand,localRotationInLeftHand);
#endif

}

[CustomEditor(typeof(ClipPlaceModule))]
public class ClipPlaceModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        GUI.enabled = true;
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("clipElement"));
        GUILayout.Space(9f);

        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localPositionInLeftHand"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localRotationInLeftHand"));
        }
            
        serializedObject.ApplyModifiedProperties();
    }
}