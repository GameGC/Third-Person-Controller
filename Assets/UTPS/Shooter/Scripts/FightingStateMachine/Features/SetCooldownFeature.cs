using System;
using System.Timers;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

[Serializable]
public class SetCooldownFeature : BaseFeature
{
    [ValidateBaseType(typeof(AnimationClip), typeof(TimelineAsset), typeof(AnimationValue))] [SerializeField]
    private Object clip;

    private IFightingStateMachineVariables _variables;
    private Timer _timer;

    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _timer = new Timer();
        _timer.AutoReset = false;
        _timer.Elapsed += TimerOnElapsed;
        _variables = variables as IFightingStateMachineVariables;
    }

    public override void OnEnterState()
    {
        float length = 0;
        switch (clip)
        {
            case AnimationClip clip: length = clip.length; break;
            case TimelineAsset timeline: length = (float) timeline.duration; break;
            case AnimationValue value: length = value.MaxLength; break;
        }
        
        _timer.Interval = length;
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