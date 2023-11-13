using UnityEngine;

namespace Weapons
{
    public class WeaponOffset : MonoBehaviour
    {
        public Vector3 localPosition;
        [QuaternionAsEuler] public Quaternion localRotation;

        private void Awake()
        {
            var thisTransform = transform;
            thisTransform.localRotation = localRotation;
            thisTransform.localPosition = localPosition;
        }
        
#if UNITY_EDITOR
        [ContextMenu("CopyVariables")]
        public void CopyVariables() => transform.GetLocalPositionAndRotation(out localPosition,out localRotation);

        [ContextMenu("PasteVariables")]
        public void PasteVariables() => transform.SetLocalPositionAndRotation(localPosition,localRotation);
#endif
    }
}