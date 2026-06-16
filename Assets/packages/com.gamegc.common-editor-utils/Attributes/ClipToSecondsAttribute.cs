using System;
using UnityEngine;

namespace GameGC.CommonEditorUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ClipToSecondsAttribute : PropertyAttribute { }
}