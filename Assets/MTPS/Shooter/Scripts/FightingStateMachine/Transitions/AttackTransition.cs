using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Input;
using UnityEngine;

namespace Fighting.Pushing
{
    public class AttackTransition : BaseStateTransition
    {
        [SerializeField] private bool isAttacking;

        private IShooterInput _input;
        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IShooterInput>();
            _variables= variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition
        {
            get
            {
                if (_input.IsAttack && isAttacking && _variables.couldAttack) return true;
                else return _input.IsAttack == isAttacking;
            }
        }
    }
}