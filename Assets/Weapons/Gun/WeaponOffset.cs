using UnityEngine;

namespace Weapons
{
    public class WeaponOffset : MonoBehaviour
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
}