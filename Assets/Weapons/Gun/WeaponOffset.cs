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
    }
}