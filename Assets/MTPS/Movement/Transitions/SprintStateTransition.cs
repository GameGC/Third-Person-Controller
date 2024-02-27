using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core.Input;
using UnityEngine;

namespace MTPS.Movement.Transitions
{
    public class SprintStateTransition : BaseStateTransition
    {
        public bool IsSprinting;
    
        private IMoveInput _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IMoveInput>();
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