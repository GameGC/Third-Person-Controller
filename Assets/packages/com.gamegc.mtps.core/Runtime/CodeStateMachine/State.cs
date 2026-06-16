using System;
using MTPS.Core.Attributes;
using UnityEngine;

namespace MTPS.Core.CodeStateMachine
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
    
        [SerializeReference,FuzzyAddButton(typeof(BaseFeature))] 
        public BaseFeature[] features = new BaseFeature[0];
    
        [SerializeReference,FuzzyAddButton(typeof(BaseStateTransition))]
        public BaseStateTransition[] Transitions = new BaseStateTransition[0];

        private int _featuresCount;
        public void CacheReferences(IStateMachineVariables variables,IReferenceResolver resolver)
        {
            _featuresCount = features.Length;
            
            for (int i = 0; i < _featuresCount; i++) 
                features[i].CacheReferences(variables, resolver);
        }
        public void OnEnterState()
        {
            for (int i = 0; i < _featuresCount; i++)
                features[i].OnEnterState();
        }

        public void OnUpdateState()
        {
            for (int i = 0; i < _featuresCount; i++)
                features[i].OnUpdateState();
        }
    
        public void OnFixedUpdateState()
        {
            for (int i = 0; i < _featuresCount; i++)
                features[i].OnFixedUpdateState();
        }

        public void OnExitState()
        {
            for (int i = 0; i < _featuresCount; i++)
                features[i].OnExitState();
        }
    }
}