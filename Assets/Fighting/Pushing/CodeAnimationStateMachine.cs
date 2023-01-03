using System;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.StateMachine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CodeAnimationStateMachine : CodeStateMachine<AnimationState>
{
    public float weight;
    public AvatarMask avatarMask;
    public bool isAdditive;


    private AnimationMixerPlayable _mixerPlayable;

    protected override void Awake()
    {
        base.Awake();
        onStateChanged.AddListener(OnStateChanged);
    }

    public AnimationMixerPlayable ConstructPlayable(PlayableGraph playableGraph)
    {
        _mixerPlayable = AnimationMixerPlayable.Create(playableGraph, states.Length);
        for (int i = 0, length =states.Length; i < length; i++)
        {
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, states[i].clip);
            playableGraph.Connect(clipPlayable, 0, _mixerPlayable, i);
        }
        _mixerPlayable.SetInputWeight(0,1);

        return _mixerPlayable;
    }

    private void OnStateChanged()
    {
        for (int i = 0, length = states.Length; i < length; i++)
        {
            _mixerPlayable.SetInputWeight(i,states[i].Name == CurrentState.Name?1:0);
        }
    }
}

[Serializable]
public class AnimationState : State
{
    public AnimationClip clip;

}