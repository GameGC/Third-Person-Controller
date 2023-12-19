using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fighting.Pushing
{
    public class LongShootTransition : BaseStateTransition
    {
        [SerializeField] private InputActionReference shootButtonReference;

        private IBaseInputReader _input;
        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
            _variables= variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => shootButtonReference.action.phase == InputActionPhase.Performed;
    }
}