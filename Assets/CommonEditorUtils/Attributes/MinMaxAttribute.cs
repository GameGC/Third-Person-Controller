using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class MinMaxAttribute : PropertyAttribute
{
    public readonly float MinLimit;
    public readonly float MaxLimit;
    public MinMaxAttribute(float min, float max)
    {
        MinLimit = min;
        MaxLimit = max;
    }
}