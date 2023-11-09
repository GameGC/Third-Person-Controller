using System;
using GameGC.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

[Serializable]
public struct CrossStateMachineValue
{
    public string mecanicState;
    public RuntimeAnimatorController Controller;
    public AnimationLayer Layer;
    public string playableState;

    public override int GetHashCode()
    {
        return (mecanicState + "-" + playableState).GetHashCode();
    }
}

public class TransitionDependedStateMachineValue : ScriptableObject
{
    /// <summary>
    /// string: playableState transition is from
    /// </summary>
    public SKeyValuePair<string, Object> states;
}