using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public abstract class ListDropdownAddDrawer
{
    public Action<object> OnAddItemFromDropdown;

    public abstract void AddDropdown(Rect buttonRect,ReorderableList list,IReferenceAddButton attribute);
}