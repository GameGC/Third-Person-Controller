using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations.Rigging.Saving;
using Object = UnityEngine.Object;

[CustomEditor(typeof(AdjustmentBehavior))]
public class AdjustmentEditor : Editor
{
    public int selectedTab1;
    public int selectedTab2
    {
        get => EditorPrefs.GetBool("AdjustmentEditor_selectedTab2") ? 1 : 0;
        set => EditorPrefs.SetBool("AdjustmentEditor_selectedTab2", value == 1);
    }

    private AdjustmentBehavior target;
    ReorderableList characterList;
    ReorderableList weapontList;

    private GameObject tab0Target;
    private Vector2 scroll;
    private Editor[] _editors;
    private bool[] folduouts;
    
    private GameObject tab1Target;

    private GUIContent isAiming = new GUIContent("Is Aiming: ");
    private GUIContent isInputFrozen = new GUIContent("Is Input Frozen: ","Freeze camera move");
    private void OnEnable()
    {
        target = base.target as AdjustmentBehavior;
        
        characterList = new ReorderableList(serializedObject, serializedObject.FindProperty("characterPrefabs"),
            false,true,true,true)
        {
            drawHeaderCallback = DrawCharacterHeaderCallback,
            drawElementCallback = DrawCharacterElement
        };

        weapontList = new ReorderableList(serializedObject, serializedObject.FindProperty("weapons"),
            false,true,true,true)
        {
            drawHeaderCallback = DrawWeaponHeaderCallback,
            drawElementCallback = DrawWeaponElement
        };
    }

    private void DrawCharacterHeaderCallback(Rect rect)
    {
        var easyGui = new EasyGUI(rect);
        easyGui.CurrentAmountSingleLine(rect.width / 1.5f, out var tempRect);
        EditorGUI.LabelField(tempRect,"Select character for setup");
      
        easyGui.CurrentAmountSingleLine(rect.width, out tempRect);
        EditorGUI.LabelField(tempRect, "Status");
    }

    private void DrawCharacterElement(Rect rect, int index, bool isactive, bool isfocused)
    {
      
        rect.y += 2;
        rect.height -= 5;
        rect.width /= 2;

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUI.PropertyField(rect, characterList.serializedProperty.GetArrayElementAtIndex(index),GUIContent.none);
        EditorGUI.EndDisabledGroup();

        rect.x += rect.width;
        if (target.selectedCharacter == index)
            EditorGUI.LabelField(rect, "Adjustment", boldStyle);
        else if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            target.SelectCharacter(index);
            target.selectedCharacter = index;
        }
    }

   
    private void DrawWeaponHeaderCallback(Rect rect)
    {
        var easyGui = new EasyGUI(rect);
        easyGui.CurrentAmountSingleLine(rect.width / 1.5f, out var tempRect);
        EditorGUI.LabelField(tempRect,"Select weapon for setup");
      
        easyGui.CurrentAmountSingleLine(rect.width, out tempRect);
        EditorGUI.LabelField(tempRect, "Status");
    }

    private void DrawWeaponElement(Rect rect, int index, bool isactive, bool isfocused)
    {
        rect.y += 2;
        rect.height -= 5;
        rect.width /= 2;

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUI.PropertyField(rect, weapontList.serializedProperty.GetArrayElementAtIndex(index),GUIContent.none);
        EditorGUI.EndDisabledGroup();

        var options = new GUIStyle {normal = {textColor = Color.green }, padding = new RectOffset(20,0,0,0)};
        rect.x += rect.width;
        if (target.selectedWeapon == index)
            EditorGUI.LabelField(rect, "Adjustment", options);
        else if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            target.SelectWeapon(index);
            target.selectedWeapon = index;
        }
    }

    private GUIStyle _boldStyle;
    private GUIStyle boldStyle
    {
        get
        {
            if (_boldStyle == null)
            {
                _boldStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    padding = new RectOffset(10, 0, 0, 0)
                };
            }

            return _boldStyle;
        }
    }

    private List<CodeStateMachine> _stateMachines = new List<CodeStateMachine>();
    private static int _selectedStateMachine;
    public override void OnInspectorGUI()
    {
        bool isPlaying = Application.isPlaying;
      
        EditorGUILayout.Space();
      
        selectedTab1 = GUILayout.Toolbar(selectedTab1, new[] {"Editor", "Settings"});
        if (selectedTab1 > 0)
        {
            DrawDefaultInspector();
            return;
        }


        var style = new GUIStyle(EditorStyles.helpBox) {richText = true, fontSize = 10};
      
        EditorGUILayout.Space();

        if (!isPlaying)
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("<b>Place here your <color=green>prefabs</color>" + ", then go to the <color=green>[Play Mode]</color> to start adjustment.</b>", style);
            EditorGUILayout.Space();
        }

        EditorGUI.BeginChangeCheck();
        var value = GUILayout.Toolbar(selectedTab2, new[] {"Characters", "Weapons"});
        if (EditorGUI.EndChangeCheck()) 
            selectedTab2 = value;

        var list = value == 0 ? characterList : weapontList;
        list.headerHeight = isPlaying ? 20 : 0;
        list.displayAdd = !isPlaying;
        list.displayRemove = !isPlaying;
        list.DoLayoutList();

        if (isPlaying)
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                target.input.IsAim = ToggleDrawer.CustomToggle(
                    GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 18f), target.input.IsAim, isAiming);
                target.input.isInputFrozen = ToggleDrawer.CustomToggle(
                    GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 18f), target.input.isInputFrozen,
                    isInputFrozen);
            }

            EditorGUILayout.Space();
        }

        if (ToggleDrawer.NeedRepaint(isAiming) || ToggleDrawer.NeedRepaint(isInputFrozen))
        {
            Repaint();
        }
        
     
        switch (value)
        {
            case 0:
            {
                if (tab0Target != target.CameraManager.EDITOR_currentCamera)
                {
                    tab0Target = target.CameraManager.EDITOR_currentCamera;

                    _editors = tab0Target.GetComponents<MonoBehaviour>().Select(CreateEditor).ToArray();
                    folduouts = new bool[_editors.Length];

                }
                int i = 0;
            
                using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(scroll))
                {
                    foreach (var editor in _editors)
                    {
                    
                        folduouts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(folduouts[i], editor.target.GetType().Name);

                        if (folduouts[i])
                        {
                            using (new EditorGUI.IndentLevelScope(1))
                            {
                                editor.OnInspectorGUI();
                            }
                        }

                        EditorGUILayout.EndFoldoutHeaderGroup();
                        i++;
                    }
                    scroll = scrollViewScope.scrollPosition;
                }
                
                EditorGUILayout.Space();
                if (GUILayout.Button("Save Camera"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Save Camera Prefab",
                        tab0Target.name + ".prefab", "prefab", "");
                    PrefabUtility.SaveAsPrefabAsset(tab0Target.gameObject, path);
                }
                break;
            }
            case 1:
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    if (GUILayout.Button("Edit Rig"))
                    {
                        if(!tab1Target)
                            tab1Target = target._currentCharacter.GetComponent<RigBuilder>().layers[(int) RigTypes.Fighting].rig.gameObject;

                        tab1Target.hideFlags = HideFlags.None;
                        Selection.activeGameObject = tab1Target;
                    }

                    if (isPlaying && GUILayout.Button("Rebuild Rig"))
                    {
                        target.RebuildRig();
                    }
                    if (GUILayout.Button("Save"))
                    {
                        SaveRig();
                    }
                }
                EditorGUILayout.Space();

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        target._currentCharacter.GetComponentsInChildren(_stateMachines);
                        _selectedStateMachine = EditorGUILayout.Popup(_selectedStateMachine,
                            _stateMachines.Select(s => s.name).ToArray());

                        if (GUILayout.Button("Edit State Machine"))
                        {
                            Selection.activeGameObject = _stateMachines[_selectedStateMachine].gameObject;
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    int stateIndex = EditorGUILayout.Popup(EditorGUIUtility.TrTextContent("State:"),
                        _stateMachines[_selectedStateMachine].CurrentStateIndex,
                        _stateMachines[_selectedStateMachine].states.Select(s => s.Name).ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        _stateMachines[_selectedStateMachine].EDITORSetStateIndex(stateIndex);
                    }
                }

                EditorGUILayout.Space();
                
                if (GUILayout.Button("Save"))
                {
                    if (target._currentCharacter.GetComponent<Inventory>().FightingStateMachine 
                        == _stateMachines[_selectedStateMachine])
                    {
                        var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                            target.weapons[target.selectedWeapon].stateMachine);
                        PrefabUtility.SaveAsPrefabAsset(_stateMachines[_selectedStateMachine].gameObject, path);
                    }
                    else
                    {
                        var path = EditorUtility.SaveFilePanelInProject("Save Character Prefab",
                            target._currentCharacter.name + ".prefab", "prefab", "");
                        PrefabUtility.SaveAsPrefabAsset(target._currentCharacter.gameObject, path);
                    }
                }
                
                break;
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void SaveRig()
    {
        if(!tab1Target)
            tab1Target = target._currentCharacter.GetComponent<RigBuilder>().layers[(int) RigTypes.Fighting].rig.gameObject;
                        
        var weaponData =
            target._currentCharacter.GetComponent<Inventory>().EquipedItemData as WeaponData;
        var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(weaponData.rigLayer);
                        
        
        foreach (var componentsInChild in tab1Target.GetComponent<Rig>().GetComponentsInChildren<IRigConstraint>())
        {
            switch (componentsInChild)
            {
                case MultiAimConstraint constraint:
                {
                    if (!constraint.GetComponent<MultiAimConstraintSaver>())
                    {
                        AddAndReset<MultiAimConstraintSaver>(constraint.gameObject);
                    }
                    break;
                }
                case MultiPositionConstraint constraint:
                {
                    if (!constraint.GetComponent<MultiPositionConstraintSaver>())
                    {
                        AddAndReset<MultiPositionConstraintSaver>(constraint.gameObject);
                    }
                    break;
                }
                case MultiRotationConstraint constraint:
                {
                    if (!constraint.GetComponent<MultiRotationConstraintSaver>())
                    {
                        AddAndReset<MultiRotationConstraintSaver>(constraint.gameObject);
                    }
                    break;
                }
                case MultiReferentialConstraint constraint:
                {
                    if (!constraint.GetComponent<MultiReferentialConstraintSaver>())
                    {
                        AddAndReset<MultiReferentialConstraintSaver>(constraint.gameObject);
                    }
                    break;
                }
                case MultiParentConstraint constraint:
                {
                    if (!constraint.GetComponent<MultiParentConstraintSaver>())
                    {
                        AddAndReset<MultiParentConstraintSaver>(constraint.gameObject);
                    }
                    break;
                }
                case TwoBoneIKConstraint constraint:
                {
                    if (!constraint.GetComponent<TwoBoneIKConstraintSaver>())
                    {
                        AddAndReset<TwoBoneIKConstraintSaver>(constraint.gameObject);
                    }
                    break;
                }
            }

            void AddAndReset<T>(GameObject constraint) where T : Component
            {
                typeof(T).GetMethod("Reset",BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(constraint.AddComponent<T>(), null);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(tab1Target, path);
    }


    const string k_OverlayId = "Scene View/Rebuilder Overlay";
    const string k_DisplayName = "Rebuilder Overlay";
    
    [Overlay(typeof(SceneView), k_OverlayId, k_DisplayName)]
    class Overlay : IMGUIOverlay, ITransientOverlay
    {
        public bool visible
        {
            get => Application.isPlaying;
        }

        public override void OnGUI()
        {
           if (GUILayout.Button("Rebuild Rig", GUILayout.Height(25)))
           {
               FindObjectOfType<AdjustmentBehavior>().RebuildRig();
           }
        }
    }
}
