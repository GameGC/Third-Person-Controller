using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[Serializable]
public class ClipValue : AnimationValue
{
    [SerializeField] private AnimationClip clip;

    public void SetClip(AnimationClip c) => clip = c;
    public override Playable GetPlayable(PlayableGraph graph,GameObject root) => AnimationClipPlayable.Create(graph, clip);
}