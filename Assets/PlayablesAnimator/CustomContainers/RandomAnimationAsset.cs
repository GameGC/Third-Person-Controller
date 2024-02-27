using System.Linq;
using GameGC.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[CreateAssetMenu(menuName = "Create RandomAnimationAsset", fileName = "RandomAnimationAsset", order = 0)]
public class RandomAnimationAsset : AnimationValue
{
    public AnimationClip[] clips;
    public bool useSequence;

    private static SKeyValueList<int, int> _iteratorsList;
    public override float MinLength => clips.Min(c => c.length);
    public override float MaxLength => clips.Max(c => c.length);

    public override Playable GetPlayable(PlayableGraph graph, GameObject root)
    {
        int length = clips.Length;
        var mixer = AnimationMixerPlayable.Create(graph, length);
        for (int i = 0; i < length; i++) 
            graph.Connect(AnimationClipPlayable.Create(graph, clips[i]), 0, mixer, i);

        if (useSequence)
        {
            _iteratorsList ??= new SKeyValueList<int, int>();
            _iteratorsList.Add(mixer.GetHandle().GetHashCode(), 0);
        }

        return mixer;
    }

    public override void OnSetWeight(Playable playable,float weight)
    {
        if(weight < 1f) return;
        int length = clips.Length;
        
        for (int i = 0; i < length; i++)
        {
            if (!(playable.GetInputWeight(i) > 0.99f)) continue;
            playable.SetInputWeight(i, 0);
            break;
        }

        if (useSequence)
        {
            var hash = playable.GetHandle().GetHashCode();
            
            var newIndex = _iteratorsList[hash];
            if (!clips[newIndex].isLooping)
                playable.GetInput(newIndex).SetTime(0);
            playable.SetInputWeight(newIndex, 1);

            newIndex++;
            if (newIndex > length - 1)
                newIndex = 0;

            _iteratorsList[hash] = newIndex;
        }
        else
        {
            int newIndex = Random.Range(0, length - 1);
            if (!clips[newIndex].isLooping)
                playable.GetInput(newIndex).SetTime(0);
            playable.SetInputWeight(newIndex, 1);
        }
    }

    public override void OnDestroyPlayable(Playable playable)
    {
        if (useSequence) 
            _iteratorsList.Remove(playable.GetHandle().GetHashCode());
    }
}
