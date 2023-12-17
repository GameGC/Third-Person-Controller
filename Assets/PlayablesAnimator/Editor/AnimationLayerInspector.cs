using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationLayer),true)]
public class AnimationLayerInspector : FollowingStateMachineEditor
{
    private AnimationLayer target;
    private int prevState;

    private readonly string[] ignorelist = new[]
    {
        "m_Script",nameof(AnimationLayer.weight),nameof(AnimationLayer.avatarMask),nameof(AnimationLayer.isAdditive),
        nameof(AnimationLayer.States), nameof(AnimationLayer.EDITOR_statesNames)
    };

    private SerializedProperty weight;
    private SerializedProperty avatarMask;
    private SerializedProperty isAdditive;

    
    protected override void OnEnable()
    {
        base.OnEnable();
        target = base.target as AnimationLayer;
        prevState = target.EDITOR_CurrentStateIndex;

        weight = serializedObject.FindProperty(nameof(AnimationLayer.weight));
        avatarMask = serializedObject.FindProperty(nameof(AnimationLayer.avatarMask));
        isAdditive = serializedObject.FindProperty(nameof(AnimationLayer.isAdditive));
    }

    public override bool RequiresConstantRepaint()
    {
        if (Application.isPlaying)
            if (prevState != target.EDITOR_CurrentStateIndex)
                return true;
        return base.RequiresConstantRepaint();
    }

    public override void OnInspectorGUI()
    {
        if (prevState != target.EDITOR_CurrentStateIndex)
        {
            prevState = target.EDITOR_CurrentStateIndex;
            UpdateVisualSelection();
        }

        EditorGUILayout.PropertyField(weight);
        EditorGUILayout.PropertyField(avatarMask);
        EditorGUILayout.PropertyField(isAdditive);

        base.DrawStateList();
        
        DrawPropertiesExcluding(serializedObject,ignorelist);
    }

    private void UpdateVisualSelection()
    {
        _list.Select(prevState);
    }
}