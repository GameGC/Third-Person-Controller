using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Code.Transitions
{
    public class SprintStateTransition : BaseStateTransition
    {
        public bool IsSprinting;
    
        private IBaseInputReader _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
        }

        public override bool couldHaveTransition
        {
            get
            {
                if (IsSprinting == false)
                {
                    if (_input.moveInput == Vector2.zero) return true;
                    return _input.isSprinting == false;
                }
                else return _input.moveInputMagnitude > 0.1f && _input.isSprinting == true;
            }
        }
    }
}