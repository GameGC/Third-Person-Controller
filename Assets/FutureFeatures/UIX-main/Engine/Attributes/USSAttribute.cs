using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedOwl.UIX.Engine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class USSAttribute : Attribute
    {
        private string _path;
        private bool _isResource;
		
        public USSAttribute(string path = "", bool isResource = false)
        {
            _path = path;
            _isResource = isResource;
        }

        public StyleSheet Load()
        {
            //string assetPath = string.IsNullOrEmpty(attr.Path) ? path : attr.Path;
            //Debug.Log($"Loading '{assetPath}.uss'");
            //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{assetPath}.uss");
            return _isResource ? Resources.Load<StyleSheet>(_path) : AssetDatabase.LoadAssetAtPath<StyleSheet>($"{_path}.uxml");
        }
    }
}