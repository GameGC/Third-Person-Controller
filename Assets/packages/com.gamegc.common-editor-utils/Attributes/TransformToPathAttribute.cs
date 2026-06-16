using System;
using UnityEngine;

namespace GameGC.CommonEditorUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TransformToPathAttribute : PropertyAttribute
    {
        public readonly Type RootType = typeof(Transform);
        public TransformToPathAttribute()
        {
      
        }
   
        public TransformToPathAttribute(Type rootType)
        {
            this.RootType = rootType;
        }
    }
}