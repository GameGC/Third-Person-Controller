using System.Linq;
using GameGC.CommonEditorUtils.Editor;
using GameGC.CommonEditorUtils.Editor.NativeReferences;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace MTPS.Core.Editor
{
    [CustomEditor(typeof(MTPS.Core.CodeStateMachine.CodeStateMachine), true)]
    public class CodeStateMachineEditor : UnityEditor.Editor
    {
        protected bool _useMinified;

        private readonly GUIContent _viewType = EditorGUIUtility.TrTextContent("View Type: ");
        private readonly GUIContent[] _viewOptions = EditorGUIUtility.TrTempContent(new[] {"Designer", "Developer"});

        protected ReorderableList StateList;

        private new MTPS.Core.CodeStateMachine.CodeStateMachine target;
        private ReorderableListWrapperRef _stateListRef;
        private int _lastStateIndex;
        
        protected virtual void OnEnable()
        {
            target = (MTPS.Core.CodeStateMachine.CodeStateMachine) base.target;
            
            var statesProperty = serializedObject.FindProperty(nameof(MTPS.Core.CodeStateMachine.CodeStateMachine.states));
            
            StateList = CreateMinifiedList(statesProperty);
            GetOrCreateListRef(statesProperty, out _stateListRef);
        }

        private static void GetOrCreateListRef(SerializedProperty property, out ReorderableListWrapperRef ref_)
        {
            var resultID = ReorderableListWrapperRef.GetPropertyIdentifier(property);
            var listElementUnCasted = PropertyHandlerRef.s_reorderableLists[resultID];

            if (listElementUnCasted == null)
            {
                ref_ = new ReorderableListWrapperRef(property,EditorGUIUtility.TrTextContent(property.displayName),true);
                var temp = PropertyHandlerRef.s_reorderableLists;
                temp.Add(resultID,ref_.originalInstance);
                PropertyHandlerRef.s_reorderableLists = temp;
            }
            else ref_ = new ReorderableListWrapperRef(listElementUnCasted);
        }
        
        protected ReorderableList CreateMinifiedList(SerializedProperty property)
        {
            var list = new ReorderableList(property.serializedObject, property);
            list.headerHeight = 0;
            list.drawElementCallback = (rect, index, active, focused) => {
                rect.y += 3f;
                rect.height -= 5f;
                
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                if (property.type == "State")
                {
                    element.NextVisible(true);
                    element.stringValue = EditorGUI.TextField(rect, element.stringValue);
                }
                else
                {
                    GUI.Label(rect,GetPropertyTypeName(element));
                }
            };
            
            if(property.type == "State")
                list.onAddCallback = list => {
                    int index = list.selectedIndices.Count > 0
                        ? list.selectedIndices[0]
                        : list.count - 1;

                    list.ClearSelection();

                    //var stateMachine = list.serializedProperty.GetPropertyParent<MTPS.Core.CodeStateMachine.CodeStateMachine>(); 
                    var states = list.serializedProperty.GetProperty<MTPS.Core.CodeStateMachine.State[]>();
                    Debug.Log(states.Length);
                    var copy = new MTPS.Core.CodeStateMachine.State(states[index]);
                    copy.Name += " (1)";
                    ArrayUtility.Insert(ref states, index + 1, copy);
                    list.serializedProperty.serializedObject.Update();
                    (target as MTPS.Core.CodeStateMachine.CodeStateMachine).OnValidate();
                };
            return list;
        }

        protected void DrawStateList(SerializedProperty property,bool minified, ReorderableList list)
        {
            if (minified)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUI.indentLevel++;
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName);
                    if (property.isExpanded)
                    {
                        list.DoLayoutList();
                        if (list.selectedIndices.Count > 0)
                        {
                            var selected = property.GetArrayElementAtIndex(list.selectedIndices[0]);
                                    
                            EditorGUILayout.LabelField(selected.displayName,EditorStyles.boldLabel);
                                    
                            selected.NextVisible(true);
                            selected.NextVisible(false);
                            EditorGUILayout.PropertyField(selected,true);
                            selected.NextVisible(false);
                            EditorGUILayout.PropertyField(selected,true);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else EditorGUILayout.PropertyField(property);
        }
        
        protected void DrawFeatureList(SerializedProperty property,bool minified, ReorderableList list)
        {
            if (minified)
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUI.indentLevel++;
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName);
                    if (property.isExpanded)
                    {
                        list.DoLayoutList();
                        if (list.selectedIndices.Count > 0)
                        {
                            var selected = property.GetArrayElementAtIndex(list.selectedIndices[0]);
                                    
                            EditorGUILayout.LabelField(GetPropertyTypeName(selected),EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(selected,true);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else EditorGUILayout.PropertyField(property);
        }

        protected void DrawViewSwitch()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(1f, 0.58f, 0f, 0.21f);
                
                GUILayout.Label(_viewType,GUILayout.Width(EditorStyles.label.CalcSize(_viewType).x));
                _useMinified = GUILayout.Toolbar(_useMinified ? 0 : 1, _viewOptions,GUI.skin.button,GUI.ToolbarButtonSize.FitToContents) < 1;
              
                GUI.backgroundColor = Color.white;
            }
            GUILayout.Space(9f);
        }
        
        protected string GetPropertyTypeName(SerializedProperty property)
        {
            string actionName = property.managedReferenceFullTypename.Split(" ").Last();

            var split = actionName.Split('.');
            if (split.Length > 2) actionName = split[^2] + '.' + split[^1];

            return actionName;
        }
        
        public override void OnInspectorGUI()
        {
            DrawViewSwitch();
            
            if (_useMinified)
            {
                var iterator = serializedObject.GetIterator();
                iterator.NextVisible(true);
                while (iterator.NextVisible(false))
                {
                    if (iterator.name == nameof(MTPS.Core.CodeStateMachine.CodeStateMachine.states))
                    {
                        DrawStateList(iterator, _useMinified, StateList);
                    }
                    else EditorGUILayout.PropertyField(iterator);
                }
            }
            else
            {
                DrawPropertiesExcluding(serializedObject,"m_Script");
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void UpdateVisualSelection()
        {
            if (Application.isPlaying && _lastStateIndex != target.CurrentStateIndex)
            {
                if( _stateListRef.m_ReorderableList == null) return;
                int index = target.CurrentStateIndex;
        
                _stateListRef.m_ReorderableList.Select(index);
                StateList.Select(index);
                _lastStateIndex = target.CurrentStateIndex;
                Repaint();
            }
        }
    }
}
