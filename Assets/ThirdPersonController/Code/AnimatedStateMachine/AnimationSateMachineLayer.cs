using System;
using UnityEngine;

public class AnimationSateMachineLayer : MonoBehaviour
{
    public float weight;
    public AvatarMask avatarMask;
    public bool isAdditive;

    public AnimationPair[] Clips;
}

[Serializable]
public struct AnimationPair
{
    public string key;
    public AnimationClip value;
}