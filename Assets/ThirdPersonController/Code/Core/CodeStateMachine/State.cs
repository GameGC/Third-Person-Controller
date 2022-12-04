using System;
using StateMachineLogic.DI;
using UnityEngine;

namespace ThirdPersonController.Core.CodeStateMachine
{
    [Serializable]
    public class State
    {
        public string Name;
    
        [SerializeReference,SerializeReferenceAddButton(typeof(BaseFeature))] 
        public BaseFeature[] features = new BaseFeature[0];
    
        [SerializeReference,SerializeReferenceAddButton(typeof(BaseStateTransition))]
        public BaseStateTransition[] Transitions = new BaseStateTransition[0];

        public void CacheReferences(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            foreach (var feature in features)
            {
                feature.CacheReferences(variables,resolver);
            }
        }
        public void OnEnterState()
        {
            foreach (var feature in features)
            {
                feature.OnEnterState();
            }
        }

        public void OnUpdateState()
        {
            foreach (var feature in features)
            {
                feature.OnUpdateState();
            }
        }
    
        public void OnFixedUpdateState()
        {
            foreach (var feature in features)
            {
                feature.OnFixedUpdateState();
            }
        }

        public void OnExitState()
        {
            foreach (var feature in features)
            {
                feature.OnExitState();
            }
        }
    }
}