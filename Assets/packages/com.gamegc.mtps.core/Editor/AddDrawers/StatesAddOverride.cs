using GameGC.CommonEditorUtils.Editor;
using MTPS.Core.Attributes;
using MTPS.Core.Editor.AddDrawers.Core;
using UnityEditor;
using UnityEditorInternal;
using State = MTPS.Core.CodeStateMachine.State;

namespace MTPS.Core.Editor.AddDrawers
{
    [AddHandlerFor(typeof(StatesAddButton))]
    public class StatesAddOverride : ListAddDrawer
    {
        public override void Add(ReorderableList list)
        {
            if (list.count > 0)
            {
                int index = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;

                list.ClearSelection();

                var stateMachine = list.serializedProperty.GetPropertyParent<CodeStateMachine.CodeStateMachine>();
                ref var states = ref stateMachine.states;

                var copy = new State(states[index]);
                copy.Name += " (1)";
                ArrayUtility.Insert(ref states, index + 1, copy);
                list.serializedProperty.serializedObject.Update();
                stateMachine.OnValidate();
            }
            else
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
            }
        }
    }
}