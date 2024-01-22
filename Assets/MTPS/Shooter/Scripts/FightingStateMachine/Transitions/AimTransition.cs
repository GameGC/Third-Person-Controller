using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;

namespace Fighting.Pushing
{
    public class AimTransition : BaseStateTransition
    {
        [SerializeField] private bool isAim;

        private IBaseInputReader _input;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
        }

        public override bool couldHaveTransition => _input.IsAim == isAim;
    }
}