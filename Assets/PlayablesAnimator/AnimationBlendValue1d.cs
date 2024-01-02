using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[CreateAssetMenu(menuName = "Create AnimationBlendValue1d", fileName = "AnimationBlendValue1d", order = 0)]
public class AnimationBlendValue1d : AnimationValue
{
    public AnimationClip[] clips;
    
    public override float MinLength => clips.Min(c => c.length);
    public override float MaxLength => clips.Max(c => c.length);
    
    
    public override Playable GetPlayable(PlayableGraph graph, GameObject root)
    {
        int length = clips.Length;
        var mixer = AnimationMixerPlayable.Create(graph, length);
        for (int i = 0; i < length; i++)
        {
            var clip = AnimationClipPlayable.Create(graph, clips[i]);
            clip.SetApplyFootIK(false);
            clip.SetApplyPlayableIK(false);
            graph.Connect(clip, 0, mixer, i);
        }

        return mixer;
    }

    public override void SetCustomVariables<T>(Playable p, T arg0)
    {
        if (arg0 is float value)
        {
            int length = clips.Length;
            var valueMap = new float[length];
            for (var i = 0; i < length; i++)
            {
                valueMap[i] = (float) i / (length-1);
            }

            bool found = false;
            for (var i = 0; i < length; i++)
            {
                if (!found && valueMap[i] >= value && value <= valueMap[Mathf.Clamp(i+1,0,length-1)])
                {
                    if (value is > 0 and < 1)
                    {
                     
                        if(value < 0.5f)
                        {
                            p.SetInputWeight(i-1, 1 -value);
                            p.SetInputWeight(i, value);
                            //Debug.Log((i-1)+" "+ (1 -value));
                            //Debug.Log(i+" "+ value);
                        }
                        else if(value > 0.5f)
                        {
                            //Debug.Log((i-1)+" "+ (1 -value));
                            //Debug.Log(i+" "+ value);
                            p.SetInputWeight(i-1, 1 -value);
                            p.SetInputWeight(i, value);
                        }
                        else if(length % 3 == 0)
                        {
                            //Debug.Log(i+" 1");
                            p.SetInputWeight(i, 1);
                        }
                        //Debug.Log((i-1)+" "+ (1 -value));
                        //Debug.Log(i+" "+ value);
                    }
                    else
                    {
                        //Debug.Log(i+" 1");
                        p.SetInputWeight(i, 1);
                    }
                    found = true;
                }
                else p.SetInputWeight(i, 0);
            }
            
            for (int i = 0; i < length; i++)
            {
                if(!clips[i].isLooping)
                    p.GetInput(i).SetTime(0);
            }
        }
    }
}
