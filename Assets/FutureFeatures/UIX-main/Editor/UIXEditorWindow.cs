using UnityEditor;

namespace RedOwl.UIX.Editor
{
    public class UIXEditorWindow : EditorWindow
    {
        private bool IsInitalized { get; set; }
        
        public virtual void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            if (IsInitalized) return;
            UIXEditor.Setup(this, rootVisualElement);
            IsInitalized = true;
        }

        public virtual void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        }

        private void OnBeforeAssemblyReload()
        {
            IsInitalized = false;
        }
    }
}