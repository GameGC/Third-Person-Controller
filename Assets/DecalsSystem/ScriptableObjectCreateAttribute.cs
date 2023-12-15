using System;
using UnityEngine;

/// <summary>
/// Attribute for context create add
/// </summary>
public class ScriptableObjectCreateAttribute : PropertyAttribute
{
   public Type BaseType = null;

   public ScriptableObjectCreateAttribute()
   {
   }

   public ScriptableObjectCreateAttribute(Type baseType)
   {
      BaseType = baseType;
   }
}