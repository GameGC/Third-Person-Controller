using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine;

namespace Fighting.Pushing
{
    public class ReloadingTransition : BaseStateTransition
    {
        [SerializeField] private bool isReloading;

        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => _variables.isReloading == isReloading;
    }
}