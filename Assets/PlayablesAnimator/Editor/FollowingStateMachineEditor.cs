using GameGC.CommonEditorUtils.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(FollowingStateMachine<>),true,isFallback = true)]
public class FollowingStateMachineEditor : Editor
{
    private bool isProperty;
    private string StatesName => isProperty ? "<States>k__BackingField" : "States";
    protected ReorderableList _list;
    private GUIContent[] _names;

    protected virtual void OnEnable()
    {
        isProperty = GetType() == typeof(FollowingStateMachineEditorManaged);
        _names = EditorGUIUtility.TrTempContent((target as dynamic).EDITOR_statesNames as string[]);
        
        var property = serializedObject.FindProperty(StatesName);
        

        _list = new ReorderableList(serializedObject, property, false, false, false, false)
        {
            elementHeight = 20f,
            drawElementCallback = DrawElementCallback,
            footerHeight = 0
        };
        if(isProperty)
            _list.elementHeightCallback = index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index))+2;
    }

    private void OnValidate() => OnEnable();

    private void DrawElementCallback(Rect rect, int index, bool active, bool focused)
    {
        float padding = rect.height > 0 ? 2 : 0;
        rect.y += padding;
        rect.height -= padding;
        var scope = new EasyGUI(rect);
        scope.CurrentHalfSingleLine( 0.5f, out var tempRect);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.Popup(tempRect, index, _names);
        EditorGUI.EndDisabledGroup();
        scope.CurrentAmountSingleLine(3, out tempRect);
        scope.CurrentHalfLine(rect.height,0.5f, out tempRect);
        
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(tempRect, _list.serializedProperty.GetArrayElementAtIndex(index), GUIContent.none, true);
        if (EditorGUI.EndChangeCheck())
        {
            _list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    //public override VisualElement CreateInspectorGUI()
    //{
    //    var root = new VisualElement();
    //    GCUIElementsUtils.FillDefaultInspectorWithIncludeByCount(root,serializedObject,this,3);
    //    //var t2 = new MultiColumnListView();
    //    var list = (VisualElement)new PropertyField(serializedObject.FindProperty("States")) as ListView;
    //    //list.bindItem += (element, i) => 
    //    return root;
    //}


    protected void DrawStateList()
    {
        _list.DoLayoutList();
    }

    public override void OnInspectorGUI()
    {
        DrawStateList();
        DrawPropertiesExcluding(serializedObject,StatesName,"m_Script");
        serializedObject.ApplyModifiedProperties();
    }
}