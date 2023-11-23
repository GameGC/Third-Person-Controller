using System;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class QAttribute : Attribute
    {
        private readonly string _name;
        private readonly bool _needsName;
        private readonly string[] _classes;
		
        public QAttribute(string name = null, params string[] classes)
        {
            _name = name;
            _needsName = string.IsNullOrEmpty(_name);
            _classes = classes;
        }

        public string GetName(string defaultValue)
        {
            return _needsName ? defaultValue : _name;
        }

        public string[] GetClasses() => _classes;
    }
}