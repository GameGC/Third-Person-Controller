using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Input;

namespace ThirdPersonController.MovementStateMachine.Code.Transitions
{
    public class CrouchStateTransition : BaseStateTransition
    {
        public bool IsCrouching;
    
        private BaseInputReader _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<BaseInputReader>();
        }

        public override bool couldHaveTransition => IsCrouching == _input.isCrouch;
    }
}