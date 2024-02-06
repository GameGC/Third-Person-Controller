using UnityEngine;

namespace UTPS.FightingStateMachine.Extras
{
    public class WeaponAnimatorOverride : MonoBehaviour
    {
        public AnimatorOverrideController Controller;

        private HybridAnimator _hybridAnimator;
        private void Awake()
        {
            _hybridAnimator = GetComponentInParent<HybridAnimator>();
        }

        private void OnEnable()
        {
            _hybridAnimator.MecanimOverride = Controller;
        }

        private void OnDisable()
        {
            _hybridAnimator.MecanimOverride = null;
        }
    }
}