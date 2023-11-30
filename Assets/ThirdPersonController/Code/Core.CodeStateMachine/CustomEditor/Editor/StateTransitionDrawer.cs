#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateTransitionAttribute))]
internal sealed class StateTransitionDrawer : PropertyDrawerWithCustomData<StateTransitionDrawer.DataContainer>
{
    public class DataContainer
    {
        public bool cached;
        public CodeStateMachine _stateMachine;
        public BaseStateTransition _transition;
    }


    protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label, DataContainer customData)
    {
        // base.OnGUI(position, property, label);
        if (!customData.cached)
        {
            customData._stateMachine = property.serializedObject.targetObject as CodeStateMachine; 
            customData._transition = property.GetPropertyParent<BaseStateTransition>();
            customData.cached = true;
        }

        int selected = property.intValue;
        EditorGUI.BeginChangeCheck();
        selected = EditorGUI.Popup(position,"Next State", selected, customData._stateMachine.states.Select(s => s.Name).ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            bool isDirty = customData._transition.SetNextState(ref customData._stateMachine.states,ref customData._stateMachine.states[selected]);
            if(isDirty)
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif