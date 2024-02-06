using GameGC.CommonEditorUtils.Attributes;
using UnityEditor;
using UnityEngine;

namespace Weapons
{
    [ToolBarDisplayGroup("SpawnOffset")]
    public class WeaponOffset : BaseWeaponExtension
    {
        public Vector3 localPosition;
        [QuaternionAsEuler] public Quaternion localRotation;

        public string applyWhen;
        private void Awake()
        {
            if (applyWhen == string.Empty)
            {
                Apply();
            }
        }

        public void Apply() => transform.SetLocalPositionAndRotation(localPosition,localRotation);
        public void SetCustomPose(Pose pose) => transform.SetLocalPositionAndRotation(pose.position,pose.rotation);

#if UNITY_EDITOR
        [ContextMenu("CopyVariables")]
        public void CopyVariables() => transform.GetLocalPositionAndRotation(out localPosition,out localRotation);

        [ContextMenu("PasteVariables")]
        public void PasteVariables() => transform.SetLocalPositionAndRotation(localPosition,localRotation);
#endif
    }

    [CustomEditor(typeof(WeaponOffset))]
    public class WeaponOffsetEditor : BaseWeaponExtensionEditor
    {
        public override void OnInspectorGUI()
        {
            DrawScriptHeader();

            using (new GUILayout.VerticalScope(box))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("localPosition"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("localRotation"));
            }
            GUILayout.Space(9f);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyWhen"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}