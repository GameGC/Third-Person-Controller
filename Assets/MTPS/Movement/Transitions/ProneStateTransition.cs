using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core.Input;

namespace MTPS.Movement.Transitions
{
    public class ProneStateTransition : BaseStateTransition
    {
        public bool IsProne;
    
        private IMoveInput _input;
        public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IMoveInput>();
        }

        public override bool couldHaveTransition => IsProne == _input.isProne;
    }
}