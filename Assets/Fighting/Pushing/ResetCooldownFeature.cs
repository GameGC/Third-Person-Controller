using System;
using System.Timers;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public class ResetCooldownFeature : BaseFeature
{
    public float cooldownTimer = 3;


    private IFightingStateMachineVariables _variables;
    private Timer _timer;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _timer = new Timer(cooldownTimer*1000);
        _timer.AutoReset = false;
        _timer.Elapsed += TimerOnElapsed;
        _variables = variables as IFightingStateMachineVariables;
    }

  

    public override void OnEnterState()
    {
        _timer.Enabled = true;
        _timer.Start();
    }
    
    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _variables.isCooldown = false;
    }
    
    public override void OnExitState()
    {
        _timer.Stop();
        _timer.Enabled = false;
    }
}

[Serializable]
public class SetCooldownFeature : BaseFeature
{
    public AnimationClip clip;


    private IFightingStateMachineVariables _variables;
    private Timer _timer;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _timer = new Timer(clip.length*1000);
        _timer.AutoReset = false;
        _timer.Elapsed += TimerOnElapsed;
        _variables = variables as IFightingStateMachineVariables;
    }

  

    public override void OnEnterState()
    {
        _timer.Enabled = true;
        _timer.Start();
    }
    
    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _variables.isCooldown = true;
    }
    
    public override void OnExitState()
    {
        _timer.Stop();
        _timer.Enabled = false;
    }
}