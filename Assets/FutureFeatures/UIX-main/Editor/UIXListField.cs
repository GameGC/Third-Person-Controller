using System;
using System.Collections;
using System.Reflection;
using RedOwl.UIX.Engine;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedOwl.UIX.Editor
{
    public class ListProxy
    {
        private readonly object _instance;
        private readonly FieldInfo _info;
        public readonly Type ElementType;

        public IList Items => (IList) _info.GetValue(_instance);
        
        public ListProxy(object instance, FieldInfo info)
        {
            _instance = instance;
            _info = info;
            ElementType = info.FieldType.GetElementType();
        }

        public void RemoveAt(int index)
        {
            // ??? Won't this fuckup the index?
        }
    }
    
    [UXML("UIXListField", true), USS("UIXListField", true)]
    public class UIXListField : VisualElement
    {
        private ListProxy _proxy;
        
        [UXMLElement]
        private readonly Label Label;

        [UXMLElement]
        private readonly Button AddBtn;
        
        [UXMLElement]
        private readonly ListView ListView;
        
        public UIXListField(ListProxy proxy, string label)
        {
            _proxy = proxy;
            UIXEditor.Setup(this);
            //AddToClassList("unity-base-field");

            Label.text = label;
            AddBtn.clicked += OnClickedAdd;
            ListView.makeItem += OnMakeItem;
            ListView.bindItem += OnBindItem;
            ListView.itemHeight = 22;
            ListView.itemsSource = proxy.Items;
            ListView.Rebuild();
            ListView.style.height = _proxy.Items.Count * 22;
        }



        private VisualElement OnMakeItem()
        {
            return new UIXListItem();
        }
        
        private void OnBindItem(VisualElement element, int index)
        {
            var item = _proxy.Items[index];
            UIXListItem itemView = (UIXListItem) element;
            itemView.BindItem(_proxy, index, item);
        }

        private void OnClickedAdd()
        {
            Debug.Log("Clicked Add");
        }

        private void OnClickedRemove()
        {
            Debug.Log("Clicked Remove");
        }
    }
}