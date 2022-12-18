using System;
using UnityEngine;

namespace ThirdPersonController.Core.DI.CustomEditor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ComponentSelectAttribute : PropertyAttribute
    {
    
    }
}