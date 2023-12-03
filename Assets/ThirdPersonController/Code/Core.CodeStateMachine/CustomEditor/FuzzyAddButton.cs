using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SerializeReferenceAddButton : PropertyAttribute,IReferenceAddButton
{
    public Type BaseType { get; }
    
    public SerializeReferenceAddButton(Type baseType)
    {
        BaseType = baseType;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FuzzyAddButton : PropertyAttribute,IReferenceAddButton
{
    public Type BaseType { get; }
    
    public FuzzyAddButton(Type baseType)
    {
        BaseType = baseType;
    }
}


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class StatesAddButton : PropertyAttribute , IAddButtonAttribute
{
    
}

public interface IReferenceAddButton :IAddButtonAttribute
{
    public Type BaseType { get; }
}

public interface IAddButtonAttribute
{
   
}