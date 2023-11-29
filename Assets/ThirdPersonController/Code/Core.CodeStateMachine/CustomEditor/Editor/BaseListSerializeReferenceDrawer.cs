using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using State = ThirdPersonController.Core.CodeStateMachine.State;

public abstract class BaseListSerializeReferenceDrawer<T> where T : Attribute, IReferenceAddButton
{
    protected bool IsFirstArrayElement(SerializedProperty property)
    {
        var path = property.propertyPath;
        var index0 = path.LastIndexOf('[');

        try
        {
            var currentIndex = int.Parse(path.Substring(index0 + 1, path.Length - 2 - index0));
            return currentIndex < 1;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    protected SerializedProperty GetTargetProperty(SerializedProperty property)
    {
        var separated = property.propertyPath.Split('.');
        var target = separated[^3];

        if (separated.Length - 3 > 0)
        {
            var fullPathAdd = "";
            for (int i = 0; i < separated.Length - 3; i++)
            {
                fullPathAdd += separated[i] + '.';
            }

            target = fullPathAdd + target;
        }

        return property.serializedObject.FindProperty(target);
    }

    protected delegate IReferenceAddButton GetAttributesForField(SerializedProperty property);

    protected static GetAttributesForField GetAttributesForFieldF;

    public static void Init(Type thisType)
    {
        var unityEditorCoreModule =
            Assembly.Load("UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        var scriptAttributeUtil = unityEditorCoreModule.GetType("UnityEditor.ScriptAttributeUtility");
        var getFiledInfoMethod = scriptAttributeUtil.GetMethod("GetFieldInfoAndStaticTypeFromProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        GetAttributesForFieldF = new GetAttributesForField(property =>
        {
            Type t = null;
            var filedInfo = getFiledInfoMethod.Invoke(null, new object[] {property, t}) as FieldInfo;
            return filedInfo.GetCustomAttribute<T>();
        });

        var instance = Activator.CreateInstance(thisType) as BaseListSerializeReferenceDrawer<T>;
        Selection.selectionChanged += instance.OnSelectionChanged;
        if (Selection.activeGameObject) instance.OnSelectionChanged();
    }

    private void OnSelectionChanged()
    {
        UnityEditor.Editor.finishedDefaultHeaderGUI -= OnReload;

        if(Selection.activeGameObject && !Selection.activeGameObject.TryGetComponent<CodeStateMachine>(out var st)) return;

        UnityEditor.Editor.finishedDefaultHeaderGUI += OnReload;
    }

    protected virtual void OnReload(UnityEditor.Editor obj)
    {
        Debug.Log("fuck");
        if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) return;

        //if (!Selection.activeGameObject) return;
        //if (!Selection.activeGameObject.TryGetComponent<CodeStateMachine>(out var st)) return;

        var editor = new SerializedObject(obj.serializedObject.targetObject);
        SerializedProperty iterator = editor.GetIterator();
        bool next = iterator.NextVisible(true);

        while (next)
        {
            bool couldDraw = IsFirstArrayElement(iterator);
            if (couldDraw)
            {
                var attribute = GetAttributesForFieldF(iterator);
                if (attribute != null) DoListFooter(GetTargetProperty(iterator), attribute);
            }

            next = iterator.NextVisible(true);
        }
    }

    protected void DoListFooter(SerializedProperty property, IReferenceAddButton attribute)
    {

        var resultID = ReorderableListWrapperRef.GetPropertyIdentifier(property);
        var listElementUnCasted = PropertyHandlerRef.s_reorderableLists[resultID];

        if (listElementUnCasted == null) return;
        ReorderableListWrapperRef element = new ReorderableListWrapperRef(listElementUnCasted);

        element.m_ReorderableList.onAddDropdownCallback =
            (rect, list) => OnReorderListAddDropdown(rect, list, attribute);
    }

    protected virtual void OnReorderListAddDropdown(Rect buttonRect, ReorderableList list, IReferenceAddButton attribute)
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
            menu.AddItem(new GUIContent(actionName), false, OnAddItemFromDropdown, dynamicType);
        }

        menu.ShowAsContext();
    }

    protected void OnAddItemFromDropdown(object obj)
    {
        var element = obj as Tuple<SerializedProperty, Type, int, ReorderableList, string>;

        var _tempList = element.Item1;
        if (_tempList.propertyPath != element.Item5) _tempList = _tempList.serializedObject.FindProperty(element.Item5);
        int last = element.Item3;

        _tempList.InsertArrayElementAtIndex(last);
        SerializedProperty lastProp = _tempList.GetArrayElementAtIndex(last);
        lastProp.managedReferenceValue = Activator.CreateInstance(element.Item2);

        _tempList.serializedObject.ApplyModifiedProperties();
    }

    #region Helper Methods

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

    private List<Type> GetNonAbstractTypesSubclassOf(Type parentType, bool sorted = true)
    {
        List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(type => !type.IsAbstract && (type.IsSubclassOf(parentType)|| type.GetInterfaces().Contains(parentType)))
            .ToList();

        if (sorted) types.Sort(CompareTypesNames);

        return types;
    }

    private int CompareTypesNames(Type a, Type b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal);

    private bool DoesReordListHaveElementOfType(string type, ReorderableList list)
    {
        for (int i = 0; i < list.serializedProperty.arraySize; ++i)
        {
            // this works but feels ugly. Type in the array element looks like "managedReference<actualStringType>"
            if (list.serializedProperty.GetArrayElementAtIndex(i).type.Contains(type)) return true;
        }

        return false;
    }

    #endregion
}