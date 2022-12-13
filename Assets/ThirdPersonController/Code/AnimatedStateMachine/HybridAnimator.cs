using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class HybridAnimator : MonoBehaviour
{
    public AnimationSateMachineLayer[] stateMachines;
    
   // public AnimationClip clip;
    public AvatarMask clipMask;
    
    public float weight;
    PlayableGraph playableGraph;
    AnimationLayerMixerPlayable mixerPlayable;

    void Start()
    {
        var animator = GetComponent<Animator>();
        
        
        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();
        
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        mixerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, stateMachines.Length+1);
        playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        var animatorPlayable = AnimatorControllerPlayable.Create(playableGraph, animator.runtimeAnimatorController);
     //   var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        
        mixerPlayable.SetLayerMaskFromAvatarMask(1,clipMask);
        
        
        playableGraph.Connect(animatorPlayable, 0, mixerPlayable, 0);

        for (var i = 0; i < stateMachines.Length; i++)
        {
            var stateMachine = stateMachines[i];
            playableGraph.Connect(StateMachineToPlayeble(stateMachine), 0, mixerPlayable, i+1);
            
            if(stateMachine.avatarMask)
                mixerPlayable.SetLayerMaskFromAvatarMask((uint) (i+1),stateMachine.avatarMask);
            
            mixerPlayable.SetLayerAdditive((uint) (i+1),stateMachine.isAdditive);
        }


        // Plays the Graph.
        playableGraph.Play();
    }

    private AnimationMixerPlayable StateMachineToPlayeble(AnimationSateMachineLayer stateMachine)
    {
        var mixerPlayable = AnimationMixerPlayable.Create(playableGraph, stateMachine.Clips.Length);
        for (int i = 0; i < stateMachine.Clips.Length; i++)
        {
            var clipPlayble = AnimationClipPlayable.Create(playableGraph, stateMachine.Clips[i].value);
            playableGraph.Connect(clipPlayble, 0, mixerPlayable, i);
        }
        mixerPlayable.SetInputWeight(1,1);

        return mixerPlayable;
    }

    void Update()
    {
        weight = Mathf.Clamp01(weight);
        mixerPlayable.SetInputWeight(0, 1);
        mixerPlayable.SetInputWeight(1, weight);
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }

}
