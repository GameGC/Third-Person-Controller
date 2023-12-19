using UnityEngine;
using UnityEngine.Playables;

public abstract class AnimationValue : ScriptableObject
{
    public abstract float MinLength { get; }
    public abstract float MaxLength { get; }
    
    public abstract Playable GetPlayable(PlayableGraph graph,GameObject root);
    
    /// <summary>
    /// override logic on weight changed
    /// </summary>
    public virtual void OnSetWeight(Playable playable,float weight){ }
}