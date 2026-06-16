using System;
using UnityEngine;

namespace GameGC.CommonEditorUtils.Attributes
{
   /// <summary>
   /// Attribute for context create add
   /// </summary>
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
   public class ScriptableObjectCreateAttribute : PropertyAttribute
   {
      public readonly Type BaseType;

      public ScriptableObjectCreateAttribute()
      {
      }

      public ScriptableObjectCreateAttribute(Type baseType)
      {
         BaseType = baseType;
      }
   }
}