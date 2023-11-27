using System;
using System.Linq;
using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct AnimationTransition
{
    public int stateFromIndex;
    public int stateToIndex;
    
  
    
    [Min(0)]
    public float time;

    public bool CouldBeApplyable(string prevState,string newState)
    {
        return string.Equals(stateFrom, prevState, StringComparison.InvariantCultureIgnoreCase)
            &&  string.Equals(stateTo, newState, StringComparison.InvariantCultureIgnoreCase);
    }
    
    public bool CouldBeApplyable(int prevState,int newState)
    {
        return stateFromIndex == prevState && stateToIndex == newState;
    }
    
    public string stateFrom;
    public string stateTo;
    
    internal bool Validate(in string[] keys)
    {
        return ValidateStateFrom(keys) || ValidateStateTo(keys);
    }
    private bool ValidateStateFrom(in string[] keys)
    {
        int nexIndex = Array.IndexOf(keys, stateFrom);
        if (nexIndex < 0 && stateFromIndex>-1)
        {
            stateFrom = keys[stateFromIndex];
            return true;
        }

        if (stateFromIndex < 0) 
            Debug.LogError($"Null StateFrom {stateFrom} {stateTo} {stateFromIndex} {stateTo}");

        if (nexIndex == stateFromIndex) return false;
        stateFromIndex = nexIndex;
        return true;
    }
    
    private bool ValidateStateTo(in string[] keys)
    {
        int nexIndex = Array.IndexOf(keys, stateTo);
        if (nexIndex < 0 && stateToIndex>-1)
        {
            stateTo = keys[stateToIndex];
            return true;
        }

        if (stateToIndex < 0) 
            Debug.LogError($"Null StateTo {stateFrom} {stateTo} {stateFromIndex} {stateTo}");

        if (nexIndex == stateToIndex) return false;
        stateToIndex = nexIndex;
        return true;
    }
}

[CustomPropertyDrawer(typeof(AnimationTransition))]
public class AnimationTransitionDrawer : PropertyDrawerWithCustomData<AnimationTransitionDrawer.Data>
{
    public class Data
    {
        public bool inited = false;
        public AnimationTransition target;
        public int valueA;
        public int valueB;
    }
    

    private string[] states;

    private void OnEnable(SerializedProperty p,Data customData)
    {
        customData.target = p.GetProperty<AnimationTransition>();
        states = (p.serializedObject.targetObject as AnimationLayer).States.Keys.ToArray();

        customData.valueA = Array.IndexOf(states, customData.target.stateFrom);
        customData.valueB = Array.IndexOf(states, customData.target.stateTo);
    }

    protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label, Data customData)
    {
        if (!customData.inited)
        {
            OnEnable(property,customData);
            customData.inited = true;
        }

        position.width /= 3;

        EditorGUI.BeginChangeCheck();
        customData.valueA = EditorGUI.Popup(position, customData.valueA, states);
        position.x += position.width;
        customData.valueB = EditorGUI.Popup(position, customData.valueB, states);
        if (EditorGUI.EndChangeCheck())
        {
            if (customData.valueA > -1) 
                property.FindPropertyRelative("stateFrom").stringValue = states[customData.valueA];
            if (customData.valueB > -1) 
                property.FindPropertyRelative("stateTo").stringValue = states[customData.valueB];
            property.serializedObject.ApplyModifiedProperties();
        }

        position.x += position.width;
        EditorGUIUtility.labelWidth /= 3;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("time"));
        EditorGUIUtility.labelWidth *= 3;
    }
}