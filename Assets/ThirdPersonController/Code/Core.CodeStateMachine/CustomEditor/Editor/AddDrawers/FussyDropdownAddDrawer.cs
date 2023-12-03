using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[AddHandlerFor(typeof(FuzzyAddButton))]
public class FussyDropdownAddDrawer : ListDropdownAddDrawer
{
    public override void AddDropdown(Rect buttonRect, ReorderableList list, IReferenceAddButton attribute)
    {
        if (UnityEngine.Event.current == null) return;

        var property = list.serializedProperty;
        var listCopy = list;
        
        FuzzyWindowC2.Show(buttonRect,new Vector2(200, 100), attribute.BaseType, type =>
        {
            var dynamicType = new Tuple<SerializedProperty, Type, int, ReorderableList, string>(
                property, type, property.arraySize, listCopy, property.propertyPath);
            OnAddItemFromDropdown(dynamicType);
        });
    }
}