using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SerializeReferenceAddButton : System.Attribute
{
    public Type baseType;

    public SerializeReferenceAddButton(Type baseType)
    {
        this.baseType = baseType;
    }
}
