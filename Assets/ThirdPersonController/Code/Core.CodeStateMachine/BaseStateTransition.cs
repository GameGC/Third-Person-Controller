using System;
using ThirdPersonController.Core.DI;
using UnityEditor;
using UnityEngine;

namespace ThirdPersonController.Core.CodeStateMachine
{
    [Serializable]
    public abstract class BaseStateTransition
    {
        public T GetNextState<T>(ref T[] states) where T: State => states[_transitionIndex];

        [SerializeField,StateTransition] private int _transitionIndex;

    
        // don't store in build
#if UNITY_EDITOR
        [SerializeField,HideInInspector] private string _transitionName;

        public void SetNextState(ref State[] states, ref State state)
        {
            _transitionName = state.Name;
            // update index
            ValidateTransition(ref states);
        }

        // returns is dirty
        internal bool ValidateTransition<T>(ref T[] states) where T: State
        {
            int nexIndex = ArrayUtility.FindIndex(states, s => s.Name == _transitionName);
            if (nexIndex == _transitionIndex) return false;
            _transitionIndex = nexIndex;
            return true;

        }
#endif
    
    

        public abstract void Initialise(IStateMachineVariables variables,IReferenceResolver resolver);
        public abstract bool couldHaveTransition { get; }
        
        
        // there issues in unity editor ,so this is a fix
#if UNITY_EDITOR

        [HideInInspector]
        public string path;

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
        
#endif
    }
}