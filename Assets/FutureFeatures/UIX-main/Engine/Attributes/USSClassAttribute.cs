using System;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class USSClassAttribute : Attribute
    {
        public readonly string[] Names;
		
        public USSClassAttribute(params string[] names)
        {
            Names = names;
        }
    }
}