using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;

namespace Fighting.Pushing
{
    public class CooldownTransition : BaseStateTransition
    {
        [SerializeField] private bool isCooldown;

        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => _variables.isCooldown == isCooldown;
    }
}