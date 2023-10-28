using System;
using ThirdPersonController.Core.DI;
using UnityEngine;

namespace ThirdPersonController.Core.CodeStateMachine
{
    [Serializable]
    public class State
    {
        public State()
        {
        }


#if UNITY_EDITOR
        //constructor to make valid copys without bugs
        public State(State state)
        {
            Name = state.Name;
            var arrays = new BaseFeature[state.features.Length];

            for (int i = 0; i < arrays.Length; i++)
            {
                var json = JsonUtility.ToJson(state.features[i]);
                int begin = json.IndexOf("\"path\":", StringComparison.Ordinal)+4+2+2;
                var end = begin +json.Substring(begin).IndexOf('"');
                json = json.Replace(json.Substring(begin, end - begin), "empty-f" + i);

                arrays[i] = JsonUtility.FromJson(json, state.features[i].GetType()) as BaseFeature;
            }

            features = arrays;
            
            var arraysT = new BaseStateTransition[state.Transitions.Length];

            for (int i = 0; i < arraysT.Length; i++)
            {
                var json = JsonUtility.ToJson(state.Transitions[i]);
                int begin = json.IndexOf("\"path\":", StringComparison.Ordinal)+4+2+2;
                var end = begin +json.Substring(begin).IndexOf('"');
                json = json.Replace(json.Substring(begin, end - begin), "empty-t" + i);

                arraysT[i] = JsonUtility.FromJson(json, state.Transitions[i].GetType()) as BaseStateTransition;
            }
            Transitions = arraysT;
        }
#endif

        
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