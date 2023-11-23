using System;
using UnityEditor;
using UnityEngine;

namespace FuzzySearch
{
    public class FuzzySearchPanel
    {
        public event Action<string> QueryChanged;
        private string _query = string.Empty;

        #region Styles

        private static GUIStyle _searchField;
        private static GUIStyle _searchFieldCancelButton;
        private static GUIStyle _searchFieldCancelButtonEmpty;
        private static bool _stylesCached;
    
        private static void LoadStyles()
        { 
            _searchField = new GUIStyle("SearchTextField"); 
            _searchFieldCancelButton = new GUIStyle("SearchCancelButton"); 
            _searchFieldCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");
        }
        #endregion

        public void OnGUI(Rect position)
        {
            if (!_stylesCached)
            {
                LoadStyles();
                _stylesCached = true;
            }
        
            var fieldPosition = position;
            fieldPosition.width -= 15;

            var cancelButtonPosition = position;
            cancelButtonPosition.x += position.width - 15;
            cancelButtonPosition.width = 15;

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("FuzzySearch");
            _query = EditorGUI.TextField(fieldPosition, _query, _searchField);
            if (EditorGUI.EndChangeCheck())
            {
                QueryChanged?.Invoke(_query);
            }

            if (GUI.Button(cancelButtonPosition, GUIContent.none, 
                    string.IsNullOrEmpty(_query) ? _searchFieldCancelButtonEmpty : _searchFieldCancelButton) && _query != string.Empty)
            {
                _query = string.Empty;
                QueryChanged?.Invoke(_query);
                GUIUtility.keyboardControl = 0;
            }
        }
    }
}