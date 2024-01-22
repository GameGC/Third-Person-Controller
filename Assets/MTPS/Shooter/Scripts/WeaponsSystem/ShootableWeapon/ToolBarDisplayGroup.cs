using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class ToolBarDisplayGroup : PropertyAttribute
{
    public string groupName;

    public ToolBarDisplayGroup(string groupName)
    {
        this.groupName = groupName;
    }
}