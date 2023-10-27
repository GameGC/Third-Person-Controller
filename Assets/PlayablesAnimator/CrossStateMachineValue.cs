using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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