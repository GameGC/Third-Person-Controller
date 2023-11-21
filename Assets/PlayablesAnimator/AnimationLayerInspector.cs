using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationLayer))]
public class AnimationLayerInspector : Editor
{
    private AnimationLayer target;
    private ReorderableListWrapperRef _wrapperRef;
    private string _statesId;

    private string prevState;
    private void OnEnable()
    {
        target = base.target as AnimationLayer;
        _statesId = ReorderableListWrapperRef.GetPropertyIdentifier(serializedObject.FindProperty("States").FindPropertyRelative("_keyValuePairs"));
        prevState = target.CurrentState;
    }

    public override bool RequiresConstantRepaint()
    {
        if (Application.isPlaying)
            if (prevState != target.CurrentState)
                return true;
        return base.RequiresConstantRepaint();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (prevState != target.CurrentState)
        {
            if (_wrapperRef == null)
            {
                var list = PropertyHandlerRef.s_reorderableLists[_statesId];
                if (list != null)
                    _wrapperRef = new ReorderableListWrapperRef(list);
            }

            UpdateVisualSelection();
            prevState = target.CurrentState;
        }
    }

    private void UpdateVisualSelection()
    {
        _wrapperRef.m_ReorderableList.Select(ArrayUtility.FindIndex(target.States.Keys.ToArray(),s=>s == target.CurrentState));
    }
}