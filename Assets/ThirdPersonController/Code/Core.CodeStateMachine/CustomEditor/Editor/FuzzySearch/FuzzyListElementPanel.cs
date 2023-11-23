using System;
using TypeNamespaceTree;
using UnityEngine;

namespace FuzzySearch
{
    public class FuzzyListElementPanel
    {
        public IOptionTree Tree;
        public event Action<CategoryTree> NextClicked;

        #region Styles
    
        private static GUIStyle _optionWithIcon;
        private static GUIStyle _optionWithoutIcon;
        private static GUIStyle _rightArrow;

        private static bool _stylesCached;

        private void LoadStyles()
        {
            _optionWithIcon = new GUIStyle("PR Label")
            {
                richText = true,
                alignment = TextAnchor.MiddleLeft
            };
            _optionWithIcon.padding.left -= 15;
            _optionWithIcon.fixedHeight = 20f;
        
            _optionWithoutIcon = new GUIStyle(_optionWithIcon);
            _optionWithoutIcon.padding.left += 17;
        
            _rightArrow = new GUIStyle("AC RightArrow");
        
        }
        #endregion
    
        public void OnGUI(Rect optionPosition,GUIContent option, bool isSelected)
        {
            if (!_stylesCached)
            {
                LoadStyles();
                _stylesCached = true;
            }
        
            if (Event.current.type == EventType.Repaint)
            {
                var optionStyle =option.image ? _optionWithIcon : _optionWithoutIcon;
                optionStyle.Draw(optionPosition, option, false, false, isSelected,
                    isSelected);
            }

            var right = optionPosition.xMax;

            //has next (childs)
            if (Tree is CategoryTree)
            {
                right -= 13;
                var rightArrowPosition = new Rect(right, optionPosition.y + 4, 13, 13);

                bool isNextClicked = GUI.Button(rightArrowPosition, GUIContent.none,_rightArrow);
                if(isNextClicked)
                    NextClicked?.Invoke(Tree as CategoryTree);
            }
        }
    }
}