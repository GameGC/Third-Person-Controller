using System;
using UnityEngine;

namespace GameGC.CommonEditorUtils.Attributes
{
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
}