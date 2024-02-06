using System;
using System.Linq;
using GameGC.CommonEditorUtils.Editor;
using UnityEditor;
using UnityEngine;

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
        states = (p.serializedObject.targetObject as AnimationLayer).EDITOR_statesNames.ToArray();

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