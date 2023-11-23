using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SerializeReferenceAddButton : PropertyAttribute,IReferenceAddButton
{
    public Type BaseType { get; }
    
    public SerializeReferenceAddButton(Type baseType)
    {
        BaseType = baseType;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class FuzzyAddButton : PropertyAttribute,IReferenceAddButton
{
    public Type BaseType { get; }
    
    public FuzzyAddButton(Type baseType)
    {
        BaseType = baseType;
    }
}


public interface IReferenceAddButton
{
    public Type BaseType { get; }
}