using UnityEditor;
using UnityEngine;
using Weapons;

[ToolBarDisplayGroup("Reloading"),DisallowMultipleComponent]
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

    private void OnDestroy()
    {
        if(clipElement.parent != transform)
            Destroy(clipElement.gameObject);
    }

#if UNITY_EDITOR
    [ContextMenu("CopyVariables")]
    public void CopyVariables() => clipElement.GetLocalPositionAndRotation(out localPositionInLeftHand,out localRotationInLeftHand);

    [ContextMenu("PasteVariables")]
    public void PasteVariables() => clipElement.SetLocalPositionAndRotation(localPositionInLeftHand,localRotationInLeftHand);
#endif

}

[CustomEditor(typeof(ClipPlaceModule))]
public class ClipPlaceModuleExtensionEditor : BaseWeaponExtensionEditor
{
    public override void OnInspectorGUI()
    {
        DrawScriptHeader();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("clipElement"));
        GUILayout.Space(9f);

        using (new GUILayout.VerticalScope(box))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localPositionInLeftHand"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localRotationInLeftHand"));
        }
            
        serializedObject.ApplyModifiedProperties();
    }
}