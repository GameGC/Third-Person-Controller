using System;
using UnityEngine;

namespace MTPS.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class StatesAddButton : PropertyAttribute , IAddButtonAttribute { }
}