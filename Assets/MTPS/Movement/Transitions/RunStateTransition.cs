using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core.Input;

namespace MTPS.Movement.Transitions
{
    public class RunStateTransition : BaseStateTransition
    {
        public bool IsRunning;
    
        private IMoveInput _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IMoveInput>();
        }

        public override bool couldHaveTransition => IsRunning ? _input.moveInputMagnitude > 0.99f : _input.moveInputMagnitude < 0.99f;
    }
}