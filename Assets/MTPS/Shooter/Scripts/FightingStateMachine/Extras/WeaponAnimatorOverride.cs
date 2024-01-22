using UnityEngine;

namespace UTPS.FightingStateMachine.Extras
{
    public class WeaponAnimatorOverride : MonoBehaviour
    {
        public AnimatorOverrideController Controller;

        private void Awake()
        {
            GetComponentInParent<HybridAnimator>().MecanimOverride = Controller;
        }
    }
}