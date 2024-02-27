using MTPS.Core;
using UnityEngine;

namespace ThirdPersonController.Code.AnimatedStateMachine
{
    public interface IFightingStateMachineVariables : IStateMachineVariables
    {
        public GameObject weaponInstance { get; set; }
        
        /// <summary>
        /// weapon for left hand if exist
        /// </summary>
        public GameObject secondaryWeaponInstance { get; set; }
        
        public AnimationLayer AnimationLayer { get; }

        public bool couldAttack { get; set; }
        public bool isCooldown { get; set; }
        public bool isReloading { get; set; }
        public bool RequestedHolsterWeapon  { get; set; }
        public float MinAimingDistance { get; }
    }
}