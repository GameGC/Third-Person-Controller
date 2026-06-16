using System;
using UnityEngine;

namespace MTPS.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeReferenceAddButton : PropertyAttribute,IReferenceAddButton
    {
        public Type BaseType { get; }
    
        public SerializeReferenceAddButton(Type baseType)
        {
            BaseType = baseType;
        }
    }
}