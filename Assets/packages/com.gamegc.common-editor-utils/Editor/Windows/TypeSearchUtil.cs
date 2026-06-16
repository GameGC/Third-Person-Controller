using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor.Windows
{
    internal class TypeSearchUtil : EditorWindow
    {
        private TextField searchField;
        private ListView resultList;
        private Label fullNameLabel;

        private List<Type> matchedTypes = new();

        [MenuItem("Tools/Type Search Util")]
        public static void ShowWindow()
        {
            var window = GetWindow<TypeSearchUtil>();
            window.titleContent = new GUIContent("Type Search Util");
        }

        public void CreateGUI()
        {
            // Input field for type name search
            searchField = new TextField("Search by Type Name:");
            searchField.RegisterValueChangedCallback(OnSearchChanged);
            rootVisualElement.Add(searchField);

            // ListView to show matched types
            resultList = new ListView
            {
                style = { flexGrow = 1 },
                selectionType = SelectionType.Single,
                makeItem = () => new Button(),
                bindItem = (element, i) =>
                {
                    var button = (Button)element;
                    button.text = matchedTypes[i].FullName;
                    button.clicked += () =>
                    {
                        fullNameLabel.text = $"Type FullName: {matchedTypes[i].AssemblyQualifiedName}";
                        Debug.Log($"Type FullName: {matchedTypes[i].AssemblyQualifiedName}");
                    };
                }
            };

            rootVisualElement.Add(resultList);

            // Label to display the selected type's full name
            fullNameLabel = new Label("Type FullName:");
            rootVisualElement.Add(fullNameLabel);
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            string input = evt.newValue?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                // Clear results and reset label
                matchedTypes.Clear();
                resultList.itemsSource = null;
                resultList.Rebuild();
                fullNameLabel.text = "Type FullName:";
                return;
            }

            // Search types in all loaded assemblies
            matchedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); } // Handle reflection exceptions
                })
                .Where(t => t.FullName != null && t.FullName.Contains(input, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Update ListView with search results
            resultList.itemsSource = matchedTypes;
            resultList.Rebuild();
        }
    }
}