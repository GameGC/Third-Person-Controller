using System;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WindowMaxSizeAttribute : Attribute
    {
        public readonly float Width;
        public readonly float Height;
		
        public WindowMaxSizeAttribute(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}