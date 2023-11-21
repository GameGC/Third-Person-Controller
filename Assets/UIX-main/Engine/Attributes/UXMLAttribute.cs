using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UXMLAttribute : Attribute
    {
        private string _path;
        private bool _isResource;

        public UXMLAttribute(string path = "", bool isResource = false)
        {
            _path = path;
            _isResource = isResource;
        }

        public VisualTreeAsset Load()
        {
            return _isResource ? Resources.Load<VisualTreeAsset>(_path) : AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{_path}.uxml");
        }
    }
}