using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[CreateAssetMenu(menuName = "Create RandomAnimationAsset", fileName = "RandomAnimationAsset", order = 0)]
public class RandomAnimationAsset : AnimationValue
{
    public AnimationClip[] clips;

    public override float MinLength => clips.Min(c => c.length);
    public override float MaxLength => clips.Max(c => c.length);

    public override Playable GetPlayable(PlayableGraph graph, GameObject root)
    {
        int length = clips.Length;
        var mixer = AnimationMixerPlayable.Create(graph, length);
        for (int i = 0; i < length; i++) 
            graph.Connect(AnimationClipPlayable.Create(graph, clips[i]), 0, mixer, i);

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

        int newIndex = Random.Range(0, length - 1);
        if (!clips[newIndex].isLooping) 
            playable.GetInput(newIndex).SetTime(0);
        playable.SetInputWeight(newIndex,1);
    }
}
