using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core;

namespace MTPS.Movement.Transitions
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