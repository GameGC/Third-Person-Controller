using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.CodeStateMachine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace ThirdPersonController.Core.StateMachine
{
    public class CodeStateMachine : MonoBehaviour
    {
        [SerializeField] protected ReferenceResolver ReferenceResolver;
        protected IStateMachineVariables Variables;

    
    
    
#if UNITY_EDITOR
        public
#else
    [SerializeField] private
#endif
            State[] states;

        private State _currentState;

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
        
            _currentState = states[0];
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
                        isDirty = codeStateTransition.ValidateTransition(ref states);
                }
            }

            statesProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if(isDirty)
                EditorUtility.SetDirty(this);
        }
#endif

        private void Start()
        {
            _currentState.OnEnterState();
        }

        protected virtual void Update()
        {
            _currentState.OnUpdateState();
            foreach (var transition in _currentState.Transitions)
            {
                if (transition.couldHaveTransition)
                {
                    _currentState.OnExitState();
                    _currentState = transition.GetNextState(ref states);
                    _currentState.OnEnterState();
                    onStateChanged.Invoke();
                    break;
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            _currentState.OnFixedUpdateState();
        }
    }
}