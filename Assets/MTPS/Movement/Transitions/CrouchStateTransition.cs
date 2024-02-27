using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core.Input;
using UnityEngine;

namespace MTPS.Movement.Transitions
{
    public class CrouchStateTransition : BaseStateTransition
    {
        [SerializeField] private bool IsCrouching;
    
        private IMoveInput _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IMoveInput>();
        }

        public override bool couldHaveTransition => IsCrouching == _input.isCrouch;
    }
}