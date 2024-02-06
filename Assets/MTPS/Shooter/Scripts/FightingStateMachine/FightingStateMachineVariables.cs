using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;

namespace Fighting.Pushing
{
    [DisallowMultipleComponent]
    public class FightingStateMachineVariables : MonoBehaviour,IFightingStateMachineVariables
    {
        public GameObject weaponInstance { get; set; }
        
        /// <summary>
        /// weapon for left hand if exist
        /// </summary>
        public GameObject secondaryWeaponInstance { get; set; }
        
        public AnimationLayer AnimationLayer { get; private set; }
        
        public bool couldAttack { get; set; } = true;
        public bool isCooldown { get; set; }
        public bool isReloading { get; set; }
        public bool RequestedHolsterWeapon  { get; set; }
        
        [field:SerializeField,Tooltip("Min distance from wall to aim")]
        public float MinAimingDistance { get; private set; } = Mathf.Infinity;
        private void Awake() => AnimationLayer = GetComponent<AnimationLayer>();
    }
}