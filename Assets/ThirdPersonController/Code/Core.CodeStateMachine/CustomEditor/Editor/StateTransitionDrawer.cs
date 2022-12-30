using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateTransitionAttribute))]
internal sealed class StateTransitionDrawer : PropertyDrawerWithCustomData<StateTransitionDrawer.DataContainer>
{
    public class DataContainer
    {
        public bool cached;
        public ICStateMachine _stateMachine;
        public BaseStateTransition _transition;
    }
    
   
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, DataContainer customData)
    {
        // base.OnGUI(position, property, label);
        if (!customData.cached)
        {
            customData._stateMachine = (ICStateMachine) property.serializedObject.targetObject; 
            customData._transition = GetParent(property) as BaseStateTransition;
            customData.cached = true;
        }

        int selected = property.intValue;
        EditorGUI.BeginChangeCheck();
        selected = EditorGUI.Popup(position,"Next State", selected, customData._stateMachine.GetStates().Select(s => s.Name).ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            var stateCopy = customData._stateMachine.GetStates();
            var sel = stateCopy[selected];
            
            customData._transition.SetNextState(ref stateCopy,ref sel);
            property.serializedObject.ApplyModifiedProperties();
        }

    }


    public object GetParent(SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach(var element in elements.Take(elements.Length-1))
        {
            if(element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }
        return obj;
    }
 
    public object GetValue(object source, string name)
    {
        if(source == null)
            return null;
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if(f == null)
        {
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if(p == null)
                return null;
            return p.GetValue(source, null);
        }
        return f.GetValue(source);
    }
 
    public object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enm = enumerable.GetEnumerator();
        while(index-- >= 0)
            enm.MoveNext();
        return enm.Current;
    }
}