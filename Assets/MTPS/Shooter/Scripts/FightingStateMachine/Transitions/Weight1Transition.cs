using System.Threading.Tasks;
using MTPS.Core;
using MTPS.Core.CodeStateMachine;

namespace MTPS.Shooter.FightingStateMachine.Transitions
{
    public class Weight1Transition : BaseStateTransition
    {
        private AnimationLayer _layer;
        private bool _wasStarted;

        private Task waitTask;
    
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _layer = (variables as IFightingStateMachineVariables).AnimationLayer;
        }

    
        public override bool couldHaveTransition
        {
            get
            {
                if (!_wasStarted)
                {
                    waitTask = _layer.WaitForNextState();
                    _wasStarted = true;
                }

                if (waitTask.IsCompleted)
                {
                    waitTask = null;
                    _wasStarted = false;
                    return true;
                }
                return false;
            }
        }
    
    
    }
}