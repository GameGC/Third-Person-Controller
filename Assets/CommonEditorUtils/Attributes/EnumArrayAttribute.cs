using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class EnumArrayAttribute : PropertyAttribute
{
    public readonly Type EnumType;

    public EnumArrayAttribute(Type enumType) => EnumType = enumType;
}