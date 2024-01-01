using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[AddHandlerFor(typeof(SerializeReferenceAddButton))]
[CustomPropertyDrawer(typeof(SerializeReferenceAddButton))]
public class SimpleDropdownAddDrawer : ListDropdownAddDrawer
{
    //private void DrawSingleGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    if (property.managedReferenceValue == null)
    //    {
    //        var types = GetNonAbstractTypesSubclassOf((attribute as SerializeReferenceAddButton).BaseType).Select(t=>t.Name).ToArray();
    //        ArrayUtility.Insert(ref types,0,"null");
    //        EditorGUI.Popup(position, 0, types);
    //    }
    //}

    public override void AddDropdown(Rect buttonRect, ReorderableList list, IReferenceAddButton attribute)
    {
        var menu = new GenericMenu();

        List<Type> showTypes = GetNonAbstractTypesSubclassOf(attribute.BaseType);

        foreach (var type in showTypes)
        {
            string actionName = type.Name;

            // UX improvement: If no elements are available the add button should be faded out or
            // just not visible.
            //bool alreadyHasIt = DoesReordListHaveElementOfType(actionName,list);
            //if (alreadyHasIt)
            //    continue;

            InsertSpaceBeforeCaps(ref actionName);

            var copy = list.serializedProperty;
            var dynamicType =
                new Tuple<SerializedProperty, Type, int, ReorderableList, string>(copy, type, list.count, list,
                    copy.propertyPath);
            menu.AddItem(new GUIContent(actionName), false, OnAddItemFromDropdown.Invoke, dynamicType);
        }

        menu.ShowAsContext();
    }
    
    private List<Type> GetNonAbstractTypesSubclassOf(Type parentType, bool sorted = true)
    {
        List<Type> types = AllTypesContainer.AllTypes
            .FindAll(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(parentType));

        if (sorted) types.Sort(CompareTypesNames);

        return types;
    }
    private int CompareTypesNames(Type a, Type b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal);
    
    private void InsertSpaceBeforeCaps(ref string theString)
    {
        for (int i = 0; i < theString.Length; ++i)
        {
            char currChar = theString[i];

            if (char.IsUpper(currChar))
            {
                theString = theString.Insert(i, " ");
                ++i;
            }
        }
    }
}