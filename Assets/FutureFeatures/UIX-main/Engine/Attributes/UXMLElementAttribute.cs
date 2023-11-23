using System;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class UXMLElementAttribute : Attribute
    {
        private readonly string _name;
        private readonly bool _needsName;
		
        public UXMLElementAttribute(string name = "")
        {
            _name = name;
            _needsName = string.IsNullOrEmpty(_name);
        }

        public string GetName(string defaultValue = "")
        {
            return _needsName ? defaultValue : _name;
        }
    }
}