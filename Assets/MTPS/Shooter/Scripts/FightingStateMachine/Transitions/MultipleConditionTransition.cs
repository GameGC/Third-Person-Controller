﻿using System;
using System.Linq;
using MTPS.Core;
using MTPS.Core.Attributes;
using MTPS.Core.CodeStateMachine;
using MTPS.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Transitions
{
    [Serializable]
    public class MultipleConditionTransition : BaseStateTransition
    {
        [Header("When call conditions true")]
    
    
        [SerializeReference,SerializeReferenceAddButton(typeof(BaseStateTransition))]
        public BaseStateTransition[] Transitions = Array.Empty<BaseStateTransition>();

        public void OnValidate()
        {
            for (var i = 0; i < Transitions.Length; i++)
            {
                Transitions[i].path = $"{path}.Transitions.Array.data[{i}]";
            }
        }
    
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            foreach (var transition in Transitions)
            {
                try
                {
                    transition.Initialise(variables,resolver);
                }
                catch (Exception e)
                {
                    Debug.LogError("tr path:"+transition?.path+" "+e);
                    Debug.LogError((variables as Component).name);
                }
            }
        }

        public override bool couldHaveTransition 
        {
            get { return Transitions.All(t => t.couldHaveTransition); }
        }
    }

    [CustomPropertyDrawer(typeof(MultipleConditionTransition),true)]
    public class MultipleConditionTransitionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            BaseTransitionDrawer.hideDestinationState = true;
            float value = EditorGUI.GetPropertyHeight(property,label,true);
            BaseTransitionDrawer.hideDestinationState = false;
            return value;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));
        
            BaseTransitionDrawer.hideDestinationState = true;
            EditorGUI.PropertyField(position, property, label, true);
            BaseTransitionDrawer.hideDestinationState = false;
        }
     
        private void DrawExclude(Rect position,
            SerializedProperty property,
            GUIContent label,
            bool includeChildren)
        {
            var header = position;
            header.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(header, property, label, false);
        }
        private string GetPropertyTypeName(SerializedProperty property)
        {
            string actionName = property.managedReferenceFullTypename.Split(" ").Last();

            var split = actionName.Split('.');
            if (split.Length > 2) 
                actionName = split[^2] + '.' + split[^1];

            return actionName;
        }
    }
}