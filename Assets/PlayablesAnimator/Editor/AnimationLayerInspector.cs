using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationLayer),true)]
public class AnimationLayerInspector : FollowingStateMachineEditor
{
    private AnimationLayer target;
    private int prevState;

    private readonly string[] ignorelist = new[]
    {
        "m_Script","weight","avatarMask","isAdditive",
        nameof(AnimationLayer.States), nameof(AnimationLayer.EDITOR_statesNames)
    };

    private SerializedProperty weight;
    private SerializedProperty avatarMask;
    private SerializedProperty isAdditive;

    
    protected override void OnEnable()
    {
        base.OnEnable();
        target = base.target as AnimationLayer;
        prevState = target.CurrentStateIndex;

        weight = serializedObject.FindProperty("weight");
        avatarMask = serializedObject.FindProperty("avatarMask");
        isAdditive = serializedObject.FindProperty("isAdditive");
    }

    public override bool RequiresConstantRepaint()
    {
        if (Application.isPlaying)
            if (prevState != target.CurrentStateIndex)
                return true;
        return base.RequiresConstantRepaint();
    }

    public override void OnInspectorGUI()
    {
        if (prevState != target.CurrentStateIndex)
        {
            prevState = target.CurrentStateIndex;
            UpdateVisualSelection();
        }

        EditorGUILayout.PropertyField(weight);
        EditorGUILayout.PropertyField(avatarMask);
        EditorGUILayout.PropertyField(isAdditive);

        base.DrawStateList();
        
        DrawPropertiesExcluding(serializedObject,ignorelist);
        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateVisualSelection()
    {
        _list.Select(prevState);
    }
}