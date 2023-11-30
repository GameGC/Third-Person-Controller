using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.CodeStateMachine;

namespace ThirdPersonController.MovementStateMachine.Code.Transitions
{
    public class GroundedStateTransition : BaseStateTransition
    {
        public bool shouldBeGrounded;
    
        private IMoveStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;
        }

        public override bool couldHaveTransition => _variables.IsGrounded == shouldBeGrounded;
    }
}