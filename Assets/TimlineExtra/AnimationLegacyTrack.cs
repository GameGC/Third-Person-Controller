using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
[TrackClipType(typeof(AnimationLegacyAsset))]
[TrackBindingType(typeof(Animation))]
[TrackColor(0.75F, 0.0f, 0.0f)]
public class AnimationLegacyTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as AnimationLegacyAsset;
            if (myAsset)
            {
                myAsset.isBlendIn = clip.hasBlendIn;
                myAsset.easeInDuration = myAsset.isBlendIn ? (float) clip.blendInDuration : (float)clip.easeInDuration;
                
                myAsset.isBlendOut = clip.hasBlendOut;
                myAsset.easeOutStartTime = myAsset.isBlendOut ? (float)(clip.duration-clip.blendOutDuration):(float) clip.easeOutTime;
            }
        }
 
        return base.CreateTrackMixer(graph, go, inputCount);
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        var asset = ((AnimationLegacyAsset) clip.asset).clip;
        if (asset)
        {
            clip.duration = asset.length;
        }
    }
}