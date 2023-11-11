using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomPropertyDrawer(typeof(CrossStateMachineValue))]
public class CrossStateMachineValueEditor : PropertyDrawer
{
    private CrossStateMachineValue target;

    private List<string> macanimStates = new List<string>();
    private string[] playbleStates;

    private int valueA;
    private int valueB;

    private bool inited = false;
    private void OnEnable(SerializedProperty p)
    {
        target = p.GetProperty<CrossStateMachineValue>();
        AnimatorController controller = null;
        if (target.Controller is AnimatorController c) controller = c; 
        else if (target.Controller is AnimatorOverrideController ov)
            controller = ov.runtimeAnimatorController as AnimatorController;

        GetAllStates(controller.layers[0].stateMachine.states,macanimStates);
        GetAllStates(controller.layers[0].stateMachine.stateMachines,macanimStates);

        playbleStates = target.Layer.States.Keys.ToArray();
        
        valueA = macanimStates.IndexOf(target.mecanicState);
        valueB = Array.IndexOf(playbleStates,target.playableState);

    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!inited)
        {
            OnEnable(property);
        }

        EditorGUI.BeginChangeCheck();
        position.width /= 2;
        valueA = EditorGUI.Popup(position, valueA, macanimStates.ToArray());
        position.x += position.width;
        valueB = EditorGUI.Popup(position, valueB, target.Layer.States.Keys.ToArray());

        if (EditorGUI.EndChangeCheck())
        {
            property.FindPropertyRelative(nameof(CrossStateMachineValue.mecanicState)).stringValue = macanimStates[valueA];
            property.FindPropertyRelative(nameof(CrossStateMachineValue.playableState)).stringValue =
                target.Layer.States.Keys.ToArray()[valueB];
            
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void GetAllStates(ChildAnimatorState[] states,List<string> statesResult)
    {
        statesResult.AddRange(states.Select(s => s.state.name));
    }
    
    private void GetAllStates(ChildAnimatorStateMachine[] stateMachines,List<string> statesResult)
    {
        foreach (var childAnimatorStateMachine in stateMachines)
        {
            var childStates = childAnimatorStateMachine.stateMachine.states;
            if (childStates.Length > 0)
                GetAllStates(childStates, statesResult);
            var childStateMachines = childAnimatorStateMachine.stateMachine.stateMachines;
            if(childStateMachines.Length > 0)
                GetAllStates(childStateMachines, statesResult);
        }
    }
}

public static class GCUtils
{
    public static T GetProperty<T>(this SerializedProperty prop) => (T) GetProperty(prop);
    public static object GetProperty(this SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }

        return obj;
    }

    public static T GetPropertyParent<T>(this SerializedProperty prop) => (T) GetPropertyParent(prop);
    public static object GetPropertyParent(this SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements.Take(elements.Length - 1))
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }

        return obj;
    }

    private static object GetValue(object source, string name)
    {
        if (source == null) return null;
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null)
        {
            var p = type.GetProperty(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null) return null;
            return p.GetValue(source, null);
        }

        return f.GetValue(source);
    }

    private static object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enm = enumerable.GetEnumerator();
        while (index-- >= 0) enm.MoveNext();
        return enm.Current;
    }
}