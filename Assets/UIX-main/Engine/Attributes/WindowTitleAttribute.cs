using System;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WindowTitleAttribute : Attribute
    {
        public readonly string Title;
		
        public WindowTitleAttribute(string title = "Window")
        {
            Title = title;
        }
    }
}