using GameGC.CommonEditorUtils.Editor;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEditorInternal;

[AddHandlerFor(typeof(StatesAddButton))]
public class StatesAddOverride : ListAddDrawer
{
    public override void Add(ReorderableList list)
    {
        if (list.count > 0)
        {
            int index = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;

            list.ClearSelection();

            var stateMachine = list.serializedProperty.GetPropertyParent<CodeStateMachine>();
            ref var states = ref stateMachine.states;

            var copy = new ThirdPersonController.Core.CodeStateMachine.State(states[index]);
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