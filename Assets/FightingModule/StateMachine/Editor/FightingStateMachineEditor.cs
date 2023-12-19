using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using State = ThirdPersonController.Core.CodeStateMachine.State;

[CustomEditor(typeof(FightingStateMachine))]
public class FightingStateMachineEditor : Editor
{
    public Texture2D fire;
    public Texture2D reload;
    public Texture2D idle;
    public Texture2D[] overrideTextures;
    public Texture2DArray Texture2DArray;
        
    private new FightingStateMachine target;

    private readonly GUIContent[] _toolBarContents = EditorGUIUtility.TrTempContent(new[] {"Basic","Get / Put Back Weapon", "States","All States"});
    
    private static int _windowId;
    private const string windowIdSavePath = nameof(FightingStateMachineEditor) + "/" + nameof(_windowId);
    
    private SerializedProperty _states;
    
    private ReorderableList _stateListbasic;
    private ReorderableList _stateList;
    private void OnEnable()
    {
        target = base.target as FightingStateMachine;
        _windowId = EditorPrefs.GetInt(windowIdSavePath);
        _states = serializedObject.FindProperty("states");


        _statesId = ReorderableListWrapperRef.GetPropertyIdentifier(_states);
        if (Application.isPlaying && target.CurrentState != null) 
            prevState = target.CurrentState.Name;
    }

    private void OnDisable()
    {
        EditorPrefs.SetInt(windowIdSavePath,_windowId);
    }
    
    private string prevState;
    private ReorderableListWrapperRef _wrapperRef;
    private string _statesId;

    public override bool RequiresConstantRepaint()
    {
        if (Application.isPlaying)
            if (prevState != target.CurrentState.Name)
                return true;
        return base.RequiresConstantRepaint();
    }
    
    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject,nameof(FightingStateMachine.states),nameof(FightingStateMachine.hasPutWeaponBackState),nameof(FightingStateMachine.hasGetWeaponState));
        _windowId = GUILayout.Toolbar(_windowId, _toolBarContents);
        switch (_windowId)
        {
            case 0:
            {
                if (_stateListbasic == null)
                {
                    _stateListbasic = new ReorderableList(serializedObject, _states, true, true, true, true);
                    _stateListbasic.drawElementCallback += DrawElementCallbackBasic;
                    _stateListbasic.headerHeight = 0;
                    _stateListbasic.elementHeightCallback += index =>
                        EditorGUI.GetPropertyHeight(_states.GetArrayElementAtIndex(index).FindPropertyRelative("features"), true);
                    _stateListbasic.onAddCallback += list =>
                    {
                        int index = list.selectedIndices.Count > 0
                            ? list.selectedIndices[0]
                            : list.count - 1;

                        list.ClearSelection();


                        var stateMachine = list.serializedProperty.GetPropertyParent<CodeStateMachine>();
                        ref var states = ref stateMachine.states;

                        var copy = new State(states[index]);
                        copy.Name += " (1)";
                        ArrayUtility.Insert(ref states, index + 1, copy);
                        list.serializedProperty.serializedObject.Update();
                        stateMachine.OnValidate();
                    };
                    
                }
                _stateListbasic.DoLayoutList();
                break;
            }
            case 1:
            {
                EditorGUI.BeginChangeCheck();
                var hasEnterState = GUILayout.Toggle(target.hasGetWeaponState, "Has Enter / Get Weapon State");
                if (EditorGUI.EndChangeCheck())
                {
                    bool changed = hasEnterState != target.hasGetWeaponState;
                    
                    var stateMachine = _states.GetPropertyParent<CodeStateMachine>();
                    ref var states = ref stateMachine.states;
                    
                    if (changed && hasEnterState)
                    {
                        var copy = new State
                        {
                            Name = "GetWeapon"
                        };
                        ArrayUtility.Insert(ref states,0,copy);
                        stateMachine.OnValidate();
                    }
                    else if (changed)
                    {
                        ArrayUtility.RemoveAt(ref states,0);
                    }

                    target.hasGetWeaponState = hasEnterState;
                    _states.serializedObject.Update();
                    EditorUtility.SetDirty(target);
                }

                if ( target.hasGetWeaponState) 
                    DrawSingleProperty(_states.GetArrayElementAtIndex(0));
                
                EditorGUI.BeginChangeCheck();
                var hasExitState = GUILayout.Toggle(target.hasPutWeaponBackState, "Has Exit / Put Back Weapon State");
                if (EditorGUI.EndChangeCheck())
                {
                    bool changed = hasExitState != target.hasPutWeaponBackState;
                    if (changed && hasExitState)
                    {
                        var stateMachine = _states.GetPropertyParent<CodeStateMachine>();
                        ref var states = ref stateMachine.states;

                        var copy = new State
                        {
                            Name = "PutWeaponBackFromAim"
                        };
                        ArrayUtility.Insert(ref states,_states.arraySize,copy);
                        stateMachine.OnValidate();
                    }
                    else if (changed)
                        _states.DeleteArrayElementAtIndex(_states.arraySize-1);

                    target.hasPutWeaponBackState = hasExitState;
                    _states.serializedObject.Update();
                    EditorUtility.SetDirty(target);
                }
                
                if (target.hasPutWeaponBackState) 
                    DrawSingleProperty(_states.GetArrayElementAtIndex(_states.arraySize -1));

                break;
            }
            case 2:
            {
                int startIndex = target.hasGetWeaponState ? 1 : 0;
                int endIndex = target.hasPutWeaponBackState ? _states.arraySize -1 : _states.arraySize;

                if (_stateList == null)
                {
                    _stateList = new ReorderableList(serializedObject, _states,true,true,true,true);
                    _stateList.drawElementCallback += DrawElementCallback;
                    _stateList.headerHeight = 0;
                    _stateList.elementHeightCallback += index =>
                    {
                        if(index < 1 && target.hasGetWeaponState) return 0;
                        if(index == _stateList.count-1 && target.hasPutWeaponBackState) return 0;
                        return EditorGUI.GetPropertyHeight(_states.GetArrayElementAtIndex(index), true);
                    };
                    _stateList.onAddCallback += list =>
                    {
                        int index = list.selectedIndices.Count > 0
                            ? list.selectedIndices[0]
                            : list.count - 1;

                        list.ClearSelection();

                
                        var stateMachine = list.serializedProperty.GetPropertyParent<CodeStateMachine>();
                        ref var states = ref stateMachine.states;

                        var copy = new State(states[index]);
                        copy.Name += " (1)";
                        ArrayUtility.Insert(ref states,index+1,copy);
                        list.serializedProperty.serializedObject.Update();
                        stateMachine.OnValidate();
                    };
                }
                
                _stateList.DoLayoutList();

                break;
            }
            case 3:
            {
                EditorGUILayout.PropertyField(_states);
                break;
            }
        }
        serializedObject.ApplyModifiedProperties();
        
        if (Application.isPlaying && prevState != target.CurrentState.Name)
        {
            if (_wrapperRef == null)
            {
                var list = PropertyHandlerRef.s_reorderableLists[_statesId];
                if (list != null)
                    _wrapperRef = new ReorderableListWrapperRef(list);
            }

            UpdateVisualSelection();
            prevState = target.CurrentState.Name;
        }
    }

    private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
    {
        if(index < 1 && target.hasGetWeaponState) return;
        if(index == _stateList.count-1 && target.hasPutWeaponBackState) return;
        rect.x += 9;
        rect.width -= 9;
        ReorderableList.defaultBehaviours
           .DrawElement(rect,_states.GetArrayElementAtIndex(index),null,_stateList.selectedIndices.Contains(index),isfocused,true,true);
    }
    
    private void DrawElementCallbackBasic(Rect rect, int index, bool isactive, bool isfocused)
    {
        var state = _states.GetArrayElementAtIndex(index);
        rect.x += 9;
        rect.width -= 9;
        EditorGUI.PropertyField(rect, state.FindPropertyRelative("features"),new GUIContent(state.displayName,
            (state.FindPropertyRelative("Name").stringValue == "Shoot")? fire:
                (state.FindPropertyRelative("Name").stringValue == "Reload")? reload:(state.FindPropertyRelative("Name").stringValue == "Idle")? idle:null));
    }

    private void DrawSingleProperty(SerializedProperty property)
    {
        string path = property.propertyPath;
        using (new EditorGUI.IndentLevelScope(2))
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                property.NextVisible(true);
                EditorGUILayout.PropertyField(property);
                while (property.NextVisible(false) && property.isArray && property.propertyPath.StartsWith(path))
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
        }
    }
    
    private void UpdateVisualSelection()
    {
        _wrapperRef.m_ReorderableList.Select(ArrayUtility.FindIndex(target.states,s=>s == target.CurrentState));
    }
}
