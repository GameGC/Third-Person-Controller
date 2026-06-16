using System;

namespace MTPS.Core.Editor.AddDrawers.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AddHandlerForAttribute : Attribute
    {
        public Type TargetPropertyAttribute;

        public AddHandlerForAttribute(Type targetPropertyAttribute)
        {
            TargetPropertyAttribute = targetPropertyAttribute;
        }
    }
}