using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core.Input;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fighting.Pushing
{
    public class LongShootTransition : BaseStateTransition
    {
        [SerializeField] private InputActionReference shootButtonReference;

        private IMoveInput _input;
        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IMoveInput>();
            _variables= variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => shootButtonReference.action.phase == InputActionPhase.Performed;
    }
}