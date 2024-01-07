using UnityEditor;
using UnityEngine;

namespace Weapons
{
    [ToolBarDisplayGroup("Shooting")]
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

    [CustomEditor(typeof(WeaponMuzzle))]
    public class WeaponMuzzleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");
            }
        }
    }
}