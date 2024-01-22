using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;

namespace Fighting.Pushing
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