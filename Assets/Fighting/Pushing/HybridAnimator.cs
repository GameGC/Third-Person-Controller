using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class HybridAnimator : MonoBehaviour
{
   //[Serializable]
   //public abstract class BaseStateMachineLayer
   //{
   //    public float weight;
   //}
   //[Serializable]
   //public class AnimatorStateMachineLayer : BaseStateMachineLayer
   //{
   //    public Animator Animator;
   //}
   //
   //[Serializable]
   //public class CodeStateMachineLayer : BaseStateMachineLayer
   //{
   //    public AvatarMask avatarMask;
   //    public bool isAdditive;
   //    public CodeAnimationStateMachine Layer;
   //}
    
    
    public AnimationLayer[] stateMachines;

   // [SerializeReference,SerializeReferenceAddButton(typeof(BaseStateMachineLayer))]
   // public BaseStateMachineLayer[] Layers;
    
    
    PlayableGraph playableGraph;
    AnimationLayerMixerPlayable layerPlayble;

    void Start()
    {
        var animator = GetComponent<Animator>();
        
        
        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();
        
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        layerPlayble = AnimationLayerMixerPlayable.Create(playableGraph, stateMachines.Length+1);
        playableOutput.SetSourcePlayable(layerPlayble);

        // Creates AnimationClipPlayable and connects them to the mixer.
        var animatorPlayable = AnimatorControllerPlayable.Create(playableGraph, animator.runtimeAnimatorController);
     //   var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        
        //layerPlayble.SetLayerMaskFromAvatarMask(1,clipMask);
        
        
        playableGraph.Connect(animatorPlayable, 0, layerPlayble, 0);

        for (var i = 1; i < stateMachines.Length+1; i++)
        {
            var stateMachine = stateMachines[i-1];
            playableGraph.Connect(stateMachine.ConstructPlayable(playableGraph,gameObject), 0, layerPlayble, i);
            
            if(stateMachine.avatarMask)
                layerPlayble.SetLayerMaskFromAvatarMask((uint)i,stateMachine.avatarMask);
            
            layerPlayble.SetLayerAdditive((uint) i,stateMachine.isAdditive);
            layerPlayble.SetInputWeight(i,stateMachine.weight);
        }


        layerPlayble.SetInputWeight(0,1);
        layerPlayble.SetInputWeight(1,1);
        // Plays the Graph.
        playableGraph.Play();
    }

    public void Rebuild(int i)
    {
        layerPlayble.DisconnectInput(i);
        
        var stateMachine = stateMachines[i-1];
        if(!stateMachine) return;
        playableGraph.Connect(stateMachine.ConstructPlayable(playableGraph,gameObject), 0, layerPlayble, i);
            
        if(stateMachine.avatarMask)
            layerPlayble.SetLayerMaskFromAvatarMask((uint)i,stateMachine.avatarMask);
            
        layerPlayble.SetLayerAdditive((uint) i,stateMachine.isAdditive);
        layerPlayble.SetInputWeight(i,stateMachine.weight);
    }
   
    //private AnimationMixerPlayable StateMachineToPlayeble(CodeAnimationStateMachine stateMachine)
    //{
    //    var mixerPlayable = AnimationMixerPlayable.Create(playableGraph, stateMachine.states.Length);
    //    for (int i = 0; i < stateMachine.states.Length; i++)
    //    {
    //        var clipPlayble = AnimationClipPlayable.Create(playableGraph, stateMachine.states[i].clip);
    //        playableGraph.Connect(clipPlayble, 0, mixerPlayable, i);
    //    }
    //    mixerPlayable.SetInputWeight(1,1);
//
    //    return mixerPlayable;
    //}
    
    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }

}
