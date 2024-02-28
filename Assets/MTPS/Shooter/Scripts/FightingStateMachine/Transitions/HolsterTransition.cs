using MTPS.Core;
using MTPS.Core.CodeStateMachine;

namespace MTPS.Shooter.FightingStateMachine.Transitions
{
    public class HolsterTransition : BaseStateTransition
    {
        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables= variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => _variables.RequestedHolsterWeapon;
    }
}