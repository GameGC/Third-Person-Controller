using MTPS.Core.Editor;
using UnityEditor;
using UnityEditorInternal;

namespace MTPS.Movement.Core.StateMachine.Editor
{
    [CustomEditor(typeof(MoveStateMachine))]
    public class MoveStateMachineEditor : CodeStateMachineEditor
    {
        private ReorderableList _featureList;
        
        protected override void OnEnable()
        {
            var featuresProperty = serializedObject.FindProperty(nameof(MoveStateMachine.alwaysExecutedFeatures));
            _featureList = CreateMinifiedList(featuresProperty);
            base.OnEnable();
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
                    else if(iterator.name == nameof(MoveStateMachine.alwaysExecutedFeatures))
                    {
                        DrawFeatureList(iterator,_useMinified,_featureList);
                    }
                    else EditorGUILayout.PropertyField(iterator);
                }
            }
            else
            {
                DrawPropertiesExcluding(serializedObject,"m_Script");
            }

            UpdateVisualSelection();
            serializedObject.ApplyModifiedProperties();
        }
    }
}