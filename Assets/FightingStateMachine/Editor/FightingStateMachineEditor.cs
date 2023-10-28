using System;
using System.Collections;
using System.Collections.Generic;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using State = ThirdPersonController.Core.CodeStateMachine.State;

[CustomEditor(typeof(FightingStateMachine))]
public class FightingStateMachineEditor : Editor
{
    private new FightingStateMachine target;

    private readonly GUIContent[] _toolBarContents = EditorGUIUtility.TrTempContent(new[] {"Get / Put Back Weapon", "States","All States"});
    
    private static int _windowId;
    private const string windowIdSavePath = nameof(FightingStateMachineEditor) + "/" + nameof(_windowId);
    
    private SerializedProperty _states;
    private ReorderableList _stateList;
    private void OnEnable()
    {
        target = base.target as FightingStateMachine;
        _windowId = EditorPrefs.GetInt(windowIdSavePath);
        _states = serializedObject.FindProperty("states");
    }

    private void OnDisable()
    {
        EditorPrefs.SetInt(windowIdSavePath,_windowId);
    }

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject,nameof(FightingStateMachine.states),nameof(FightingStateMachine.hasPutWeaponBackState),nameof(FightingStateMachine.hasGetWeaponState));
        _windowId = GUILayout.Toolbar(_windowId, _toolBarContents);
        switch (_windowId)
        {
            case 0:
            {
                EditorGUI.BeginChangeCheck();
                var hasEnterState = GUILayout.Toggle(target.hasGetWeaponState, "Has Enter / Get Weapon State");
                if (EditorGUI.EndChangeCheck())
                {
                    if (hasEnterState && !target.hasGetWeaponState)
                    {
                        var stateMachine = _states.GetPropertyParent<CodeStateMachine>();
                        ref var states = ref stateMachine.states;

                        var copy = new State
                        {
                            Name = "GetWeapon"
                        };
                        ArrayUtility.Insert(ref states,0,copy);
                        _states.serializedObject.Update();
                        stateMachine.OnValidate();
                    }
                    else if(!hasEnterState && target.hasGetWeaponState) 
                        _states.DeleteArrayElementAtIndex(0);

                    target.hasGetWeaponState = hasEnterState;
                    EditorUtility.SetDirty(target);
                }

                if (hasEnterState) 
                    DrawSingleProperty(_states.GetArrayElementAtIndex(0));
                
                EditorGUI.BeginChangeCheck();
                var hasExitState = GUILayout.Toggle(target.hasPutWeaponBackState, "Has Exit / Put Back Weapon State");
                if (EditorGUI.EndChangeCheck())
                {
                    if (hasEnterState && !target.hasGetWeaponState)
                    {
                        var stateMachine = _states.GetPropertyParent<CodeStateMachine>();
                        ref var states = ref stateMachine.states;

                        var copy = new State
                        {
                            Name = "PutWeaponBackFromAim"
                        };
                        ArrayUtility.Insert(ref states,_states.arraySize-1,copy);
                        _states.serializedObject.Update();
                        stateMachine.OnValidate();
                    }
                    else if(!hasEnterState && target.hasGetWeaponState) 
                        _states.DeleteArrayElementAtIndex(_states.arraySize-1);

                    target.hasGetWeaponState = hasEnterState;
                    EditorUtility.SetDirty(target);
                }
                
                if (hasExitState) 
                    DrawSingleProperty(_states.GetArrayElementAtIndex(_states.arraySize -1));

                break;
            }
            case 1:
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
            case 2:
            {
                EditorGUILayout.PropertyField(_states);
                break;
            }
        }
        serializedObject.ApplyModifiedProperties();
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

    private void DrawSingleProperty(SerializedProperty property)
    {
        using (new EditorGUI.IndentLevelScope(2))
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                property.NextVisible(true);
                EditorGUILayout.PropertyField(property);
                while (property.NextVisible(false) && property.isArray) 
                    EditorGUILayout.PropertyField(property);
            }
        }
    }
}
