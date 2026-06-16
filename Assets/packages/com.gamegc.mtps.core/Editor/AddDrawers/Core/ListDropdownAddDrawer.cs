using System;
using MTPS.Core.Attributes;
using UnityEditorInternal;
using UnityEngine;

namespace MTPS.Core.Editor.AddDrawers.Core
{
    public abstract class ListDropdownAddDrawer
    {
        public Action<object> OnAddItemFromDropdown;

        public abstract void AddDropdown(Rect buttonRect,ReorderableList list,IReferenceAddButton attribute);
    }
}