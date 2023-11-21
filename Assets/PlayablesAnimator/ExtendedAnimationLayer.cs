using System.Collections;
using GameGC.Collections;
using UnityEditor;
using UnityEngine;

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

    protected override IEnumerator AsyncTransition(string prevState, string newState)
    {
        OverrideClips(newState);
        return base.AsyncTransition(prevState, newState);
    }

    protected override void SyncedTransition(string previosState, string newState)
    {
        OverrideClips(newState);
        base.SyncedTransition(previosState, newState);
    }

    private void OverrideClips(string newState)
    {
        //discard previous
        var state = _mecanicAnimator.GetCurrentAnimatorStateInfo(0);
        int OverrideIndex = ArrayUtility.FindIndex(overrideStates,s=>s.Key.playableState == CurrentState && state.IsName(s.Key.mecanicState));
        if (OverrideIndex > -1)
            GetComponentInParent<HybridAnimator>().DiscardAnimClip(overrideStates[OverrideIndex].Value);

        //override new one
        state = _mecanicAnimator.GetCurrentAnimatorStateInfo(0);
        OverrideIndex = ArrayUtility.FindIndex(overrideStates,s=>s.Key.playableState == newState && state.IsName(s.Key.mecanicState));
        if (OverrideIndex > -1)
        {
            var  lips = _mecanicAnimator.GetCurrentAnimatorClipInfo(0);
            GetComponentInParent<HybridAnimator>().OverrideAnimClip(lips[0].clip,overrideStates[OverrideIndex].Value);
        }
    }
}