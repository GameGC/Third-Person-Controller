using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;

namespace ThirdPersonController.MovementStateMachine.Code.Transitions
{
    public class RunStateTransition : BaseStateTransition
    {
        public bool IsRunning;
    
        private IBaseInputReader _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
        }

        public override bool couldHaveTransition => IsRunning ? _input.moveInputMagnitude > 0.99f : _input.moveInputMagnitude < 0.99f;
    }
}