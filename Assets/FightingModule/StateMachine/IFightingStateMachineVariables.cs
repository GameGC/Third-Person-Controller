using ThirdPersonController.Core.DI;
using UnityEngine;

namespace ThirdPersonController.Code.AnimatedStateMachine
{
    public interface IFightingStateMachineVariables : IStateMachineVariables
    {
        public GameObject weaponInstance { get; set; }
        
        public bool couldAttack { get; set; }
        public bool isCooldown { get; set; }
        public bool isReloading { get; set; }
        public bool RequestedHolsterWeapon  { get; set; }

    }
}