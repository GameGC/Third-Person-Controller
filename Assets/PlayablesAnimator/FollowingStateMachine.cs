using System;
using System.Collections.Generic;
using System.Linq;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


[ExecuteAlways]
public abstract class FollowingStateMachineAbstract<T> :MonoBehaviour
{
    public abstract T[] States { get; protected set; }
    
#if UNITY_EDITOR
    [HideInInspector]
    [FormerlySerializedAs("EDITORstatesNames")]
    [FormerlySerializedAs("statesNames")] 
    [SerializeField] public string[] EDITOR_statesNames = Array.Empty<string>();

    public int EDITOR_CurrentStateIndex => CurrentStateIndex;
#endif
    protected int CurrentStateIndex;

    protected CodeStateMachine _codeStateMachine;

    protected virtual void Awake()
    {
        _codeStateMachine = GetComponent<CodeStateMachine>();
    }

    protected virtual void Reset() => OnValidate();

    protected virtual void OnValidate()
    {
        if(Application.isPlaying) return;
        _codeStateMachine ??= GetComponent<CodeStateMachine>();
        States ??= Array.Empty<T>();
        int sourceLength = _codeStateMachine.states.Length;
        int currentLength = States.Length;

        bool isDirty = false;
        if (currentLength != sourceLength)
        {
            isDirty = true;
            if (EDITOR_statesNames.Length != currentLength)
            {
                var copy = States;
                Array.Resize(ref copy,sourceLength);
                States = copy;
                EDITOR_statesNames = _codeStateMachine.states.Select(s => s.Name).ToArray();
            }
            else
            {
                var tempStateNames= new List<string>(EDITOR_statesNames)
                {
                    Capacity = sourceLength
                };
                var tempStateList = new List<T>(States)
                {
                    Capacity = sourceLength
                };

                for (int i = 0; i < sourceLength; i++)
                {
                    ref var state = ref _codeStateMachine.states[i];
                    int nameIndex = tempStateNames.IndexOf(state.Name);
                    
                    //coping new state
                    if (nameIndex < 0)
                    {
                        tempStateNames.Insert(i,state.Name);
                        tempStateList.Insert(i,default);
                    }
                    //reordering states
                    else if(nameIndex != i)
                    {
                        (tempStateNames[nameIndex], tempStateNames[i]) = (tempStateNames[i], tempStateNames[nameIndex]);
                        (tempStateList[nameIndex], tempStateList[i]) = (tempStateList[i], tempStateList[nameIndex]);
                    }
                }
                currentLength = States.Length;
                if (sourceLength != currentLength)
                {
                    for (int i = 0; i < currentLength; i++)
                    {
                        int nameIndex = Array.FindIndex(_codeStateMachine.states, s=>s.Name == tempStateNames[i]);
                        if (nameIndex < 0)
                        {
                            tempStateNames.RemoveAt(i);
                            tempStateList.RemoveAt(i);
                        }
                    }
                }

                EDITOR_statesNames = tempStateNames.ToArray();
                States = tempStateList.ToArray();
                tempStateNames.Clear();
                tempStateList.Clear();
            }
        }
        else
        {
            for (int i = 0; i < sourceLength; i++)
            {
                if (EDITOR_statesNames[i] == _codeStateMachine.states[i].Name) continue;
                isDirty = true;
                int reorderIndex = Array.FindIndex(_codeStateMachine.states, s => s.Name == EDITOR_statesNames[i]);
                (EDITOR_statesNames[reorderIndex], EDITOR_statesNames[i]) = (EDITOR_statesNames[i], EDITOR_statesNames[reorderIndex]);
                (States[reorderIndex], States[i]) = (States[i], States[reorderIndex]);
            }
        }
        
        if(isDirty)
            EditorUtility.SetDirty(this);
    }
    private static Type GetT2()
    {
        return typeof(T);
    }
}


[ExecuteInEditMode]
public abstract class FollowingStateMachine<T> :MonoBehaviour
{
    //[DrawerAsDictionaryStates]
    public T[] States = Array.Empty<T>();
    
#if UNITY_EDITOR
   //[HideInInspector]
    [FormerlySerializedAs("EDITORstatesNames")]
    [FormerlySerializedAs("statesNames")] 
    [SerializeField] public string[] EDITOR_statesNames = Array.Empty<string>();
#endif
    
    public int CurrentStateIndex { get; protected set; }

    protected CodeStateMachine _codeStateMachine;
    protected virtual void Awake()
    {
        _codeStateMachine = GetComponent<CodeStateMachine>();
    }

    protected virtual void Reset() => OnValidate();

    private void Update() => OnValidate();

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            _codeStateMachine ??= GetComponent<CodeStateMachine>();
            _codeStateMachine.EDITOR_OnValidate += OnValidate;
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            _codeStateMachine ??= GetComponent<CodeStateMachine>();
            _codeStateMachine.EDITOR_OnValidate -= OnValidate;
        }
    }
    protected virtual void OnValidate()
    {
        if(Application.isPlaying) return;
        _codeStateMachine ??= GetComponent<CodeStateMachine>();

        if (_codeStateMachine == null) return;
        int sourceLength = _codeStateMachine.states.Length;
        int currentLength = States.Length;

        bool isDirty = false;
        if (currentLength != sourceLength)
        {
            isDirty = true;
            if (EDITOR_statesNames.Length != currentLength)
            {
                Array.Resize(ref States,sourceLength);
                EDITOR_statesNames = _codeStateMachine.states.Select(s => s.Name).ToArray();
            }
            else
            {
                var tempStateNames= new List<string>(EDITOR_statesNames)
                {
                    Capacity = Mathf.Max(EDITOR_statesNames.Length,sourceLength)
                };
                var tempStateList = new List<T>(States)
                {
                    Capacity = Mathf.Max(EDITOR_statesNames.Length,sourceLength)
                };

                for (int i = 0; i < sourceLength; i++)
                {
                    ref var state = ref _codeStateMachine.states[i];
                    int nameIndex = tempStateNames.IndexOf(state.Name);
                    
                    //coping new state
                    if (nameIndex < 0)
                    {
                        tempStateNames.Insert(i,state.Name);
                        tempStateList.Insert(i,default);
                    }
                    //reordering states
                    else if(nameIndex != i)
                    {
                        (tempStateNames[nameIndex], tempStateNames[i]) = (tempStateNames[i], tempStateNames[nameIndex]);
                        (tempStateList[nameIndex], tempStateList[i]) = (tempStateList[i], tempStateList[nameIndex]);
                    }
                }
                currentLength = States.Length;
                if (sourceLength != currentLength)
                {
                    for (int i = 0; i < currentLength; i++)
                    {
                        int nameIndex = Array.FindIndex(_codeStateMachine.states, s=>s.Name == tempStateNames[i]);
                        if (nameIndex < 0)
                        {
                            tempStateNames.RemoveAt(i);
                            tempStateList.RemoveAt(i);
                        }
                    }
                }

                EDITOR_statesNames = tempStateNames.ToArray();
                States = tempStateList.ToArray();
                tempStateNames.Clear();
                tempStateList.Clear();
            }
        }
        else
        {
            for (int i = 0; i < sourceLength; i++)
            {
                if (EDITOR_statesNames[i] == _codeStateMachine.states[i].Name) continue;
                isDirty = true;
                int reorderIndex = Array.FindIndex(_codeStateMachine.states, s => s.Name == EDITOR_statesNames[i]);
                if (reorderIndex > -1)
                {
                    (EDITOR_statesNames[reorderIndex], EDITOR_statesNames[i]) =
                        (EDITOR_statesNames[i], EDITOR_statesNames[reorderIndex]);
                    (States[reorderIndex], States[i]) = (States[i], States[reorderIndex]);
                }
                else
                {
                    EDITOR_statesNames[i] = _codeStateMachine.states[i].Name;
                }
            }
        }
        
        if(isDirty)
            EditorUtility.SetDirty(this);
    }
#endif

}