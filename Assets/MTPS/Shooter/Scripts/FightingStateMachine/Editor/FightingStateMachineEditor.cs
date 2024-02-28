using MTPS.Core.CodeStateMachine;
using MTPS.Core.Editor;
using MTPS.Shooter.FightingStateMachine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using State = MTPS.Core.CodeStateMachine.State;

[CustomEditor(typeof(FightingStateMachine))]
public class FightingStateMachineEditor : CodeStateMachineEditor
{
    public Texture2D fire;
    public Texture2D reload;
    public Texture2D idle;
    public Texture2D[] overrideTextures;
        
    private new FightingStateMachine target;

    private readonly GUIContent[] _toolBarContents = EditorGUIUtility.TrTempContent(new[] {"Get / Put Back Weapon", "All States"});
    
    private static int _windowId;
    private const string windowIdSavePath = nameof(FightingStateMachineEditor) + "/" + nameof(_windowId);
    
    private SerializedProperty _states;
    
    private ReorderableList _stateListbasic;

    protected override void OnEnable()
    {
        base.OnEnable();
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
            case 1:
            {
                DrawViewSwitch();

                DrawStateList(_states, _useMinified, StateList);
                break;
            }
        }
        
        UpdateVisualSelection();
        serializedObject.ApplyModifiedProperties();
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
}
