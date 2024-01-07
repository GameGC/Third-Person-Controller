using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

[CustomEditor(typeof(RigBuilder),true,isFallback = false)]
internal class AnimationRiggingEditor : Editor
{
    private const string TypeName = "UnityEditor.Animations.Rigging.RigBuilderEditor, " +
                                    "Unity.Animation.Rigging.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

    private Editor _editor;
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        var button = new Button(OnRebuildClick)
        {
            text = "Rebuild Rig",
            style =
            {
                display = EditorApplication.isPlayingOrWillChangePlaymode ? DisplayStyle.Flex : DisplayStyle.None
            }
        };
        EditorApplication.playModeStateChanged += OnPlaymodeChange;

        root.Add(button);
        var type = System.Type.GetType(TypeName);
        _editor = CreateEditor(target, type);
        
        VisualElement element;
        if((element = _editor.CreateInspectorGUI()) != null)
            root.Add(element);
        else
            root.Add(new IMGUIContainer(_editor.OnInspectorGUI));

        return root;
        
        void OnPlaymodeChange(PlayModeStateChange change)
        {
            if (this == null)
            {
                EditorApplication.playModeStateChanged -= OnPlaymodeChange;
                return;
            }
            button.style.display = change is PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingEditMode
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    private void OnRebuildClick()
    {
        var target = this.target as RigBuilder;
        var builder = target.GetComponent<RigBuilder>();

        var animator = target.GetComponent<Animator>();
        int prevState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        animator.enabled = false;
        
        // rebuild everything
        builder.Build();
        animator.Rebind();

        animator.enabled = true;
        if (prevState != animator.GetCurrentAnimatorStateInfo(0).shortNameHash) 
            animator.Play(prevState);
    }

    private void OnDestroy()
    {
        if(_editor)
            DestroyImmediate(_editor);
    }

    private void OnDisable()
    {
        if(_editor)
            DestroyImmediate(_editor);
    }
}
