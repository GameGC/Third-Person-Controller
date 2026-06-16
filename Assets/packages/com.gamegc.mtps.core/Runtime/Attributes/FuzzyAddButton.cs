using System;
using UnityEngine;

namespace MTPS.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FuzzyAddButton : PropertyAttribute,IReferenceAddButton
    {
        public Type BaseType { get; }
    
        public FuzzyAddButton(Type baseType)
        {
            BaseType = baseType;
        }
    }
}