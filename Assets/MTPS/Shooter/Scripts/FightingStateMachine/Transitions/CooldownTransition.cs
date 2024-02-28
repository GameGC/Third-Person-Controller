using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Transitions
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