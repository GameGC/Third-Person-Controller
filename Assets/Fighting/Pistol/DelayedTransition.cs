using System;
using System.Threading.Tasks;
using System.Timers;
using Fighting.Pushing;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public class DelayedTransition : BaseStateTransition
{
    [SerializeField,ClipToSecondsAttribute] private float delayTime;

    private bool _wasStarted;
    private bool _wasTimeFinished;

    private Timer _tempTimer;
    
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _tempTimer = new Timer(delayTime * 1000) {AutoReset = false };
        _tempTimer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _wasTimeFinished = true;
        _tempTimer.Stop();
        _tempTimer.Enabled = false;
    }
    
    public override bool couldHaveTransition
    {
        get
        {
            if (!_wasStarted)
            {
                _tempTimer.Enabled = true;
                _tempTimer.Start();
                _wasStarted = true;
            }

            if (_wasTimeFinished)
            {
                _wasTimeFinished = false;
                _wasStarted = false;
                return true;
            }

            return false;
        }
    }
    
    
}

public class EndPlayTransition : BaseStateTransition
{
    public string WaitForAnimationEnd;
    
    private AnimationLayer _layer;
    private bool _wasStarted;

    private Task waitTask;
    
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _layer = (variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
    }

    
    public override bool couldHaveTransition
    {
        get
        {
            if (!_wasStarted)
            {
                waitTask = _layer.WaitForAnimationFinish(WaitForAnimationEnd);
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

public class Weight1Transition : BaseStateTransition
{
    private AnimationLayer _layer;
    private bool _wasStarted;

    private Task waitTask;
    
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _layer = (variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
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