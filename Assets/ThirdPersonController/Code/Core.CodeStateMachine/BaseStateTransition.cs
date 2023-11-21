using System;
using ThirdPersonController.Core.DI;
using UnityEditor;
using UnityEngine;

namespace ThirdPersonController.Core.CodeStateMachine
{
    [Serializable]
    public abstract class BaseStateTransition
    {
        // there issues in unity editor ,so this is a fix
#if UNITY_EDITOR

        [HideInInspector]
        public string path;

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
        
#endif
        public State GetNextState(ref State[] states) => states[_transitionIndex];

        [SerializeField,StateTransition] private int _transitionIndex;

    
        // don't store in build
#if UNITY_EDITOR
        [HideInInspector]
        [SerializeField] private string _transitionName;

        public bool SetNextState(ref State[] states, ref State state)
        {
            _transitionName = state.Name;
            // update index
            return ValidateTransition(ref states);
        }
    
        // returns is dirty
        internal bool  ValidateTransition(ref State[] states)
        {
            int nexIndex = Array.FindIndex(states, s => s.Name == _transitionName);
            if (nexIndex < 0 && _transitionIndex>-1)
            {
                _transitionName = states[_transitionIndex].Name;
                return true;
            }

            if (_transitionIndex < 0)
            {
                var findStateBelongsTo = Array.Find(states, s => ArrayUtility.Contains(s.Transitions, this));
                Debug.LogError("Null Transition "+GetType().Name+" at state "+findStateBelongsTo.Name);
            }
            
            if (nexIndex == _transitionIndex) return false;
            _transitionIndex = nexIndex;
            return true;

        }
#endif
    
    

        public abstract void Initialise(IStateMachineVariables variables,IReferenceResolver resolver);
        public abstract bool couldHaveTransition { get; }
    }
}