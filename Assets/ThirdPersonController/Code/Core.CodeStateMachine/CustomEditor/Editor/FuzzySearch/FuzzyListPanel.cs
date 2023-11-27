using System;
using System.Linq;
using TypeNamespaceTree;
using UnityEngine;

namespace FuzzySearch
{
    public class FuzzyListPanel
    {
        public float GetHeight()
        {
            return Mathf.Clamp(_list.Length,0,13) * OptionHeight;
        }

        private const float OptionHeight = 20;

        public event Action NextClicked;
        public event Action<Type> TypeClicked;
        public IOptionTree Parent;
    
        
        private int _selectedIndex =-1;
        private FuzzyListElementPanel[] _list;
        
        private Vector2 _scroll;
        private Vector2 _prevMousePos;
    
        public void SetData(IOptionTree[] contents)
        {
            Parent = contents[0].Parent;
            int i = 0;
            _list = contents.Select(c =>
            {
                var element = new FuzzyListElementPanel();
                element.Tree = contents[i];
                element.NextClicked += ElementOnNextClicked;
                i++;
                return element;
            }).ToArray();
            _selectedIndex = -1;
        }

        private void ElementOnNextClicked(IOptionTree option)
        {
            if (option is CategoryTree categoryTree)
            {
                NextClicked?.Invoke();
                SetData(categoryTree.Childs.ToArray());
            }
            else
            {
                TypeClicked?.Invoke((option as TypeTree).Type); 
            }
        }

        public bool Repaint;
        public void OnGUI(in bool isRepaint)
        {
            using (var scrollViewScope = new GUILayout.ScrollViewScope(_scroll))
            {
                bool isMovingMouse = Event.current.type == EventType.MouseMove;
                var mousePos = Event.current.mousePosition;

                for (var i = 0; i <_list.Length; i++)
                {
                    var optionPosition = GUILayoutUtility.GetRect(16, OptionHeight, GUILayout.ExpandWidth(true));
                    if (isMovingMouse) 
                        ValidateSelectionChanged(ref i, ref optionPosition, ref mousePos);

                    _list[i].OnGUI(optionPosition,_list[i].Tree.Content, _selectedIndex == i,in isRepaint);
                }

                _scroll = scrollViewScope.scrollPosition;
            }
        }

        private void ValidateSelectionChanged(ref int i,ref Rect optionPosition,ref Vector2 mousePos)
        {
            if (optionPosition.Contains(mousePos) && mousePos != _prevMousePos && _selectedIndex != i)
            {
                Repaint = true;
                _prevMousePos = mousePos;
                _selectedIndex = i;
                Event.current.Use();
                GUIUtility.ExitGUI();
            }
        }
    }
}