using RedOwl.UIX.Engine;
using UnityEngine.UIElements;

namespace RedOwl.UIX.Editor
{
    [UXML("UIXListItem", true)]
    public class UIXListItem : VisualElement
    {
        private int _index;
        private ListProxy _proxy;
        
        [UXMLElement]
        private VisualElement FieldContainer;

        [UXMLElement]
        public readonly Button RemoveBtn;

        public UIXListItem()
        {
            UIXEditor.Setup(this);
            FieldContainer.Add(new Label("Test"));
        }

        public void BindItem(ListProxy proxy, int index, object item)
        {
            _proxy = proxy;
            RemoveBtn.clicked += OnRemoveClicked;
            
            //FieldContainer.Add(UIXFactory.CreateField(instance, info));
        }

        private void OnRemoveClicked()
        {
            _proxy.RemoveAt(_index);
        }
    }
}