using System;
using System.Linq;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.CodeStateMachine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace ThirdPersonController.Core.StateMachine
{
    public class DefaultCodeStateMachine : CodeStateMachine<State> { }


    public interface ICStateMachine<T> : ICStateMachine where T : State
    {
        public new T[] GetStates();
    }
    
    public interface ICStateMachine
    {
        public State[] GetStates();
    }
    
    public class CodeStateMachine<T> : MonoBehaviour,ICStateMachine<T> where T : State 
    {
        [SerializeField] protected ReferenceResolver ReferenceResolver;
        protected IStateMachineVariables Variables;

    
    
    
        public T[] states;
        protected T CurrentState;

        public UnityEvent onStateChanged;
    
        protected virtual void Awake()
        {
            Variables = GetComponent<IStateMachineVariables>();

            foreach (var codeState in states)
            {
                codeState.CacheReferences(Variables,ReferenceResolver);
                foreach (var codeStateTransition in codeState.Transitions)
                {
                    codeStateTransition.Initialise(Variables,ReferenceResolver);
                }
            }
        
            CurrentState = states[0];
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            bool isDirty = false;
            var statesProp = new SerializedObject(this).FindProperty("states");

            for (var s = 0; s < states.Length; s++)
            {
                var codeState = states[s];
                var features = statesProp.GetArrayElementAtIndex(s).FindPropertyRelative("features");


                for (var f = 0; f < codeState.features.Length; f++)
                {
                    codeState.features[f].path =
                        features.GetArrayElementAtIndex(f).propertyPath;
                }


                var Transitions = statesProp.GetArrayElementAtIndex(s).FindPropertyRelative("Transitions");
                for (var f = 0; f < codeState.Transitions.Length; f++)
                {
                    ref var codeStateTransition = ref codeState.Transitions[f];
                    if (codeStateTransition == null) continue;
                      
                    isDirty = codeStateTransition.ValidateTransition(ref states);
                    codeStateTransition.path =
                        Transitions.GetArrayElementAtIndex(f).propertyPath;
                    
                    
                }
                
                foreach (var codeStateTransition in codeState.Transitions)
                {
                    if (codeStateTransition != null)
                    {
                        isDirty = codeStateTransition.ValidateTransition(ref states);

                        if (codeStateTransition.GetType().Name == "MultipleConditionTransition")
                        {
                            ((dynamic) codeStateTransition).OnValidate();
                        }
                    }
                }
            }

            statesProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if(isDirty)
                EditorUtility.SetDirty(this);
        }
#endif

        private void Start()
        {
            CurrentState.OnEnterState();
        }

        protected virtual void Update()
        {
            CurrentState.OnUpdateState();
            foreach (var transition in CurrentState.Transitions)
            {
                if (transition.couldHaveTransition)
                {
                    CurrentState.OnExitState();
                    CurrentState = transition.GetNextState(ref states);
                    CurrentState.OnEnterState();
                    onStateChanged.Invoke();
                    break;
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            CurrentState.OnFixedUpdateState();
        }
        
        
        public T[] GetStates()
        {
            return states;
        }

        State[] ICStateMachine.GetStates()
        {
            return (State[])states;
        }
    }
}