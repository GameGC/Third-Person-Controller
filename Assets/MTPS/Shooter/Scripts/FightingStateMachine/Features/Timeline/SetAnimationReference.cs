using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Features.Timeline
{
    public class SetAnimationReference : SetReferenceDefault
    {
        public override void OnEnterState()
        {
            if(value == null)
                value = _variables.weaponInstance.GetComponent<Animation>();
            base.OnEnterState();
        }
    }
}