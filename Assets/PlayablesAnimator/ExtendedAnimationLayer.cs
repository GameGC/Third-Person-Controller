using GameGC.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class ExtendedAnimationLayer : AnimationLayer
{
    private Animator _mecanicAnimator;

    public SKeyValuePair<CrossStateMachineValue,AnimationClip>[] overrideStates;

    protected override void Awake()
    {
        base.Awake();
        _mecanicAnimator = GetComponentInParent<Animator>();
    }
    
    private void OnValidate()
    {
        if(!_mecanicAnimator)
            _mecanicAnimator = GetComponentInParent<Animator>();

        for (var i = 0; i < overrideStates.Length; i++)
        {
            ref var keyValuePair = ref overrideStates[i];
            keyValuePair.Key.Controller = _mecanicAnimator.runtimeAnimatorController;
            keyValuePair.Key.Layer = this;
        }
    }

    protected override void SyncedTransition()
    {
        OverrideClips();
        base.SyncedTransition();
    }

    protected override void AsyncTransition()
    {
        OverrideClips();
        base.AsyncTransition();
    }

    private void OverrideClips()
    {
        //discard previous
        var state = _mecanicAnimator.GetCurrentAnimatorStateInfo(0);
        int OverrideIndex = ArrayUtility.FindIndex(overrideStates,s=>s.Key.playableState == CurrentState && state.IsName(s.Key.mecanicState));
        if (OverrideIndex > -1)
            GetComponentInParent<HybridAnimator>().DiscardAnimClip(overrideStates[OverrideIndex].Value);

        var nextState= _codeStateMachine.CurrentState.Name;
        
        //override new one
        state = _mecanicAnimator.GetCurrentAnimatorStateInfo(0);
        OverrideIndex = ArrayUtility.FindIndex(overrideStates,s=>s.Key.playableState == nextState && state.IsName(s.Key.mecanicState));
        if (OverrideIndex > -1)
        {
            var  lips = _mecanicAnimator.GetCurrentAnimatorClipInfo(0);
            GetComponentInParent<HybridAnimator>().OverrideAnimClip(lips[0].clip,overrideStates[OverrideIndex].Value);
        }
    }
}