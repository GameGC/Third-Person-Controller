using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimelineValue : AnimationValue
{
    [SerializeField] private TimelineAsset playable;
    
    public override Playable GetPlayable(PlayableGraph graph,GameObject root) => playable.CreatePlayable(graph,root);
}