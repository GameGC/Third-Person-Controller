using System;
using UnityEngine;

namespace MTPS.Shooter.WeaponsSystem.ShootableWeapon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ToolBarDisplayGroup : PropertyAttribute
    {
        public string groupName;

        public ToolBarDisplayGroup(string groupName)
        {
            this.groupName = groupName;
        }
    }
}