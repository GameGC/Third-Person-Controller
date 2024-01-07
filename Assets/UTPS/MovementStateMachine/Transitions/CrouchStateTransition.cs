using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Code.Transitions
{
    public class CrouchStateTransition : BaseStateTransition
    {
        [SerializeField] private bool IsCrouching;
    
        private IBaseInputReader _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
        }

        public override bool couldHaveTransition => IsCrouching == _input.isCrouch;
    }
}