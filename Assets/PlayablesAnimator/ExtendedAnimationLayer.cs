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

    protected override IEnumerator AsyncTransition(int prevStateIndex, int newStateIndex)
    {
        OverrideClips(prevStateIndex,newStateIndex);
        return base.AsyncTransition(prevStateIndex, newStateIndex);
    }

    protected override void SyncedTransition(int prevStateIndex, int newStateIndex)
    {
        OverrideClips(prevStateIndex,newStateIndex);
        base.SyncedTransition(prevStateIndex, newStateIndex);
    }

    private void OverrideClips(int prevStateIndex, int newStateIndex)
    {
        //discard previous
        var state = _mecanicAnimator.GetCurrentAnimatorStateInfo(0);
        int OverrideIndex = ArrayUtility.FindIndex(overrideStates,s=>s.Key.playableStateIndex == prevStateIndex && state.IsName(s.Key.mecanicState));
        if (OverrideIndex > -1)
            GetComponentInParent<HybridAnimator>().DiscardAnimClip(overrideStates[OverrideIndex].Value);

        //override new one
        state = _mecanicAnimator.GetCurrentAnimatorStateInfo(0);
        OverrideIndex = ArrayUtility.FindIndex(overrideStates,s=>s.Key.playableStateIndex == newStateIndex && state.IsName(s.Key.mecanicState));
        if (OverrideIndex > -1)
        {
            var  lips = _mecanicAnimator.GetCurrentAnimatorClipInfo(0);
            GetComponentInParent<HybridAnimator>().OverrideAnimClip(lips[0].clip,overrideStates[OverrideIndex].Value);
        }
    }
}