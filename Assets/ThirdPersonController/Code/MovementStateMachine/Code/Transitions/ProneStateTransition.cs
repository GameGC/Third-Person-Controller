using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Input;

namespace ThirdPersonController.MovementStateMachine.Code.Transitions
{
    public class ProneStateTransition : BaseStateTransition
    {
        public bool IsProne;
    
        private BaseInputReader _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<BaseInputReader>();
        }

        public override bool couldHaveTransition => IsProne == _input.isProne;
    }
}