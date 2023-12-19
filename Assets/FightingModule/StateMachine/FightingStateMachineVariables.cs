using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;

namespace Fighting.Pushing
{
    public class FightingStateMachineVariables : MonoBehaviour,IFightingStateMachineVariables
    {
        public GameObject weaponInstance { get; set; }
        
        public bool couldAttack { get; set; } = true;
        public bool isCooldown { get; set; }
        public bool isReloading { get; set; }
        public bool RequestedHolsterWeapon  { get; set; }
    }
}