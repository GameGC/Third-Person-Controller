using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TypeSearchUtil : EditorWindow
{
    private string typeName;

    private string fullName;
    private IEnumerable<Type> _types;
    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        typeName = EditorGUILayout.TextField("Search by Type Name:", typeName);
        if(string.IsNullOrWhiteSpace(typeName))return;
        if (EditorGUI.EndChangeCheck())
        {
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.FullName.Contains(typeName)));
        }
        
        if(_types == null || _types.Count() <0) return;

        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            foreach (var type in _types)
            {
                if (GUILayout.Button(type.FullName))
                {
                    fullName = type.AssemblyQualifiedName;
                }
            }
        }

        EditorGUILayout.TextField("Type FullName:", fullName);
    }
    
    [MenuItem("Tools/Type Search Util")]
    internal static void SaveAsTemplate() => EditorWindow.GetWindow<TypeSearchUtil>().Show();

}
