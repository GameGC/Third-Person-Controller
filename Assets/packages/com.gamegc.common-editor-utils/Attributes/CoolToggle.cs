using System;
using UnityEngine;

namespace GameGC.CommonEditorUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CoolToggle : PropertyAttribute
    {
        public readonly string Title;

        public CoolToggle(string toggleTitle)
        {
            Title = toggleTitle;
        }
    }
}