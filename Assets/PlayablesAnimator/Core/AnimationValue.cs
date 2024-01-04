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
    
    public virtual void SetCustomVariables<T>(Playable p,T arg0){}
    public virtual void SetCustomVariables<T0,T1>(Playable p,T0 arg0,T1 arg1){}
    public virtual void SetCustomVariables<T0,T1,T2>(Playable p,T0 arg0,T1 arg1,T2 arg2){}
    public virtual void SetCustomVariables<T0,T1,T2,T3>(Playable p,T0 arg0,T1 arg1,T2 arg2,T3 arg3){}
}