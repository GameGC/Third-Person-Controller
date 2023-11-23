using System;
using UnityEngine;

namespace RedOwl.UIX.Engine
{
    public class ImplementationPicker : PropertyAttribute
    {
        public readonly Type type;

        public ImplementationPicker(Type type)
        {
            this.type = type;
        }
    }
}