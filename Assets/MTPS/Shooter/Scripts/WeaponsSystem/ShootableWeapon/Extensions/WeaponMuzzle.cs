using UnityEditor;
using UnityEngine;

namespace Weapons
{
    [ToolBarDisplayGroup("Shooting"),DisallowMultipleComponent]
    public class WeaponMuzzle : BaseWeaponExtension
    {
        public GameObject muzzle;
        public float muzzleTime = 1;

        public override void OnShoot()
        {
            muzzle.SetActive(true);
            Invoke(nameof(DisableMuzzle),muzzleTime);
        }
        
        private void DisableMuzzle() =>muzzle.SetActive(false);
    }

    public class BaseWeaponExtensionEditor : Editor
    {
        protected GUIStyle box => GUI.skin.box;
        
        protected void DrawScriptHeader()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;
        }
        
    }

    [CustomEditor(typeof(WeaponMuzzle))]
    public class WeaponExtensionMuzzleEditor : BaseWeaponExtensionEditor
    {
        public override void OnInspectorGUI()
        {
            DrawScriptHeader();
            
            using (new GUILayout.VerticalScope(box))
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");
            }
        }
    }
}