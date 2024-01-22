using System.Threading.Tasks;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;

public class EndPlayTransition : BaseStateTransition
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
                waitTask = _layer.WaitForLastStateFinish();
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