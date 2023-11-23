using System;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WindowMinSizeAttribute : Attribute
    {
        public readonly float Width;
        public readonly float Height;
		
        public WindowMinSizeAttribute(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}