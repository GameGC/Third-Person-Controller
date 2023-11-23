using UnityEngine.UIElements;

namespace RedOwl.UIX.Editor
{
    public abstract class UIXElement : VisualElement
    {
        protected UIXElement()
        {
            UIXEditor.Setup(this);
        }
    }
}