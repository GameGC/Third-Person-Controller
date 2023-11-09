using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

[RequireComponent(typeof(Animator))]
public class HybridAnimator : MonoBehaviour
{
    public AnimationLayer[] stateMachines;


    private Animator _animator;
    private PlayableGraph _playableGraph;
    private AnimationLayerMixerPlayable _layerPlayable;

    private void Awake() => _animator = GetComponent<Animator>();

    private void Start()
    {
        if (m_MecanimOverride)
            _animator.runtimeAnimatorController = m_MecanimOverride;
        
        
        // Creates the graph, the mixer and binds them to the Animator.
        _playableGraph = PlayableGraph.Create();
        
        var playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
        _layerPlayable = AnimationLayerMixerPlayable.Create(_playableGraph, stateMachines.Length+1);
        playableOutput.SetSourcePlayable(_layerPlayable);
        

        
        // Creates AnimationClipPlayable and connects them to the mixer.
        var animatorPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);
        
        
        _playableGraph.Connect(animatorPlayable, 0, _layerPlayable, 0);

        for (var i = 1; i < stateMachines.Length+1; i++)
        {
            var stateMachine = stateMachines[i-1];
            _playableGraph.Connect(stateMachine.ConstructPlayable(_playableGraph,gameObject), 0, _layerPlayable, i);
            
            if(stateMachine.avatarMask)
                _layerPlayable.SetLayerMaskFromAvatarMask((uint)i,stateMachine.avatarMask);
            
            _layerPlayable.SetLayerAdditive((uint) i,stateMachine.isAdditive);
            _layerPlayable.SetInputWeight(i,stateMachine.weight);
        }


        _layerPlayable.SetInputWeight(0,1);
        _layerPlayable.SetInputWeight(1,1);
        // Plays the Graph.
        _playableGraph.Play();
    }

    public void Rebuild(int i)
    {
        _layerPlayable.DisconnectInput(i);
        
        var stateMachine = stateMachines[i-1];
        if(!stateMachine) return;
        _playableGraph.Connect(stateMachine.ConstructPlayable(_playableGraph,gameObject), 0, _layerPlayable, i);
            
        if(stateMachine.avatarMask)
            _layerPlayable.SetLayerMaskFromAvatarMask((uint)i,stateMachine.avatarMask);
            
        _layerPlayable.SetLayerAdditive((uint) i,stateMachine.isAdditive);
        _layerPlayable.SetInputWeight(i,stateMachine.weight);
    }

    private void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        _playableGraph.Destroy();
    }
    
    public AnimatorOverrideController MecanimOverride
    {
        get => m_MecanimOverride;
        set
        {
            _animator.runtimeAnimatorController = value;
            m_MecanimOverride = value;

            _playableGraph.Disconnect(_layerPlayable,0);
            var animatorPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);
            _playableGraph.Connect(animatorPlayable, 0, _layerPlayable, 0);
        }
    } 
    [SerializeField] private AnimatorOverrideController m_MecanimOverride;

    private List<KeyValuePair<object, AnimationClip>> _overridesCache= new(capacity:5);

    public void OverrideAnimClip(string clipName,AnimationClip newClip)
    {
        MecanimOverride ??= new AnimatorOverrideController(_animator.runtimeAnimatorController);

        AddOrSetToChangeList(clipName, m_MecanimOverride[clipName]);
        m_MecanimOverride[clipName] = newClip;
    }

    public void OverrideAnimClip(AnimationClip clip,AnimationClip newClip)
    {
        MecanimOverride ??= new AnimatorOverrideController(_animator.runtimeAnimatorController);

        AddOrSetToChangeList(clip, m_MecanimOverride[clip]);
        m_MecanimOverride[clip] = newClip;
    }


    public void DiscardAnimClip(AnimationClip clip)
    {
        if (_overridesCache.FindIndex(c =>
            {
                if (c.Key is string s) return s == clip.name;
                if (c.Key is AnimationClip a) return a == clip;
                return false;
            }, out int index))
        {
            MecanimOverride ??= new AnimatorOverrideController(_animator.runtimeAnimatorController);
            m_MecanimOverride[clip] = _overridesCache[index].Value;
            _overridesCache.RemoveAt(index);
        }
    }
    public void DiscardAnimClip(string clipName)
    {
        if (_overridesCache.FindIndex(c =>
            {
                if (c.Key is string s) return s == clipName;
                if (c.Key is AnimationClip a)
                    return string.Equals(a.name, clipName, StringComparison.OrdinalIgnoreCase);
                return false;
            }, out int index))
        {
            MecanimOverride ??= new AnimatorOverrideController(_animator.runtimeAnimatorController);
            m_MecanimOverride[clipName] = _overridesCache[index].Value;
            _overridesCache.RemoveAt(index);
        }
    }
    
    
    private void AddOrSetToChangeList(AnimationClip clip, AnimationClip newClip)
    {
        if (_overridesCache.FindIndex(c =>
            {
                if (c.Key is string s) return s == clip.name;
                if (c.Key is AnimationClip a) return a == clip;
                return false;
            }, out int index))
            _overridesCache[index] = new KeyValuePair<object, AnimationClip>(clip, newClip);
        else
            _overridesCache.Add(new KeyValuePair<object, AnimationClip>(clip,newClip));
    }
    
    private void AddOrSetToChangeList(string clipName, AnimationClip newClip)
    {
        if (_overridesCache.FindIndex(c =>
            {
                if (c.Key is string s) return s == clipName;
                if (c.Key is AnimationClip a) return string.Equals(a.name, clipName, StringComparison.OrdinalIgnoreCase);
                return false;
            }, out int index))
            _overridesCache[index] = new KeyValuePair<object, AnimationClip>(clipName, newClip);
        else
            _overridesCache.Add(new KeyValuePair<object, AnimationClip>(clipName,newClip));
    }

    public void Fuck() => Debug.LogError("fuck");
}

public static class CollectionHelpers
{
    public static bool FindIndex<T>(this List<T> list, Predicate<T> match,out int result)
    {
        result = list.FindIndex(match);
        return result > -1;
    }
}
