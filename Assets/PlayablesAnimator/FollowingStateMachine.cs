using System;
using System.Collections.Generic;
using System.Linq;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

[CustomEditor(typeof(FollowingStateMachine<>),true,isFallback = true)]
public class FollowingStateMachineEditor : Editor
{
    private bool isProperty;
    private string StatesName => isProperty ? "<States>k__BackingField" : "States";
    protected ReorderableList _list;
    private GUIContent[] _names;

    protected virtual void OnEnable()
    {
        isProperty = GetType() == typeof(FollowingStateMachineEditorManaged);
        _names = EditorGUIUtility.TrTempContent((target as dynamic).EDITOR_statesNames as string[]);
        
        var property = serializedObject.FindProperty(StatesName);
        

        _list = new ReorderableList(serializedObject, property, false, false, false, false)
        {
            elementHeight = 20f,
            drawElementCallback = DrawElementCallback,
            footerHeight = 0
        };
        if(isProperty)
            _list.elementHeightCallback = index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index))+2;
    }
    private void DrawElementCallback(Rect rect, int index, bool active, bool focused)
    {
        float padding = (rect.height > 0 ? 2 : 0);
        rect.y += padding;
        rect.height -= padding;
        var scope = new EasyGUI(rect);
        scope.CurrentHalfSingleLine( 0.5f, out var tempRect);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.Popup(tempRect, index, _names);
        EditorGUI.EndDisabledGroup();
        scope.CurrentAmountSingleLine(3, out tempRect);
        scope.CurrentHalfLine(rect.height,0.5f, out tempRect);
        
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(tempRect, _list.serializedProperty.GetArrayElementAtIndex(index), GUIContent.none, true);
        if (EditorGUI.EndChangeCheck())
        {
            _list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }
    
    
    protected void DrawStateList()
    {
        _list.DoLayoutList();
    }

    public override void OnInspectorGUI()
    {
        DrawStateList();
        DrawPropertiesExcluding(serializedObject,StatesName,"m_Script");
    }
}


[CustomEditor(typeof(FollowingStateMachineManaged<>),true,isFallback = true)]
public class FollowingStateMachineEditorManaged : FollowingStateMachineEditor
{
    
}

public abstract class FollowingStateMachineManaged<T> :MonoBehaviour
{
    [field: SerializeReference]
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



public abstract class FollowingStateMachine<T> :MonoBehaviour
{
    //[DrawerAsDictionaryStates]
    public T[] States = Array.Empty<T>();
    
#if UNITY_EDITOR
   //[HideInInspector]
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
}