#if UNITY_EDITOR
using System.Linq;
using GameGC.CommonEditorUtils.Editor;
using MTPS.Core.Attributes;
using MTPS.Core.CodeStateMachine;
using UnityEditor;
using UnityEngine;

namespace MTPS.Core.Editor
{
    [CustomPropertyDrawer(typeof(StateTransitionAttribute))]
    internal sealed class StateTransitionDrawer : PropertyDrawerWithCustomData<StateTransitionDrawer.DataContainer>
    {
        public class DataContainer
        {
            public bool cached;
            public CodeStateMachine.CodeStateMachine _stateMachine;
            public BaseStateTransition _transition;
        }


        protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label, DataContainer customData)
        {
            // base.OnGUI(position, property, label);
            if (!customData.cached)
            {
                customData._stateMachine = property.serializedObject.targetObject as CodeStateMachine.CodeStateMachine; 
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
}
#endif