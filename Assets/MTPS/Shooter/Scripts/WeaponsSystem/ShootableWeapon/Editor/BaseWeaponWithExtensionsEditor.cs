using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cinemachine.Editor;
using GameGC.CommonEditorUtils.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseWeaponWithExtensions),true,isFallback = true)]
public class BaseWeaponWithExtensionsEditor : Editor
{
    private class Tab
    {
        public string name;
        public List<Editor> editors = new List<Editor>(capacity:3);

        public Tab(string name) => this.name = name;
    }
    private static readonly List<Type> ExtensionTypes;
    private static readonly GUIContent[] ExtensionNames;

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            _selectedTab = value;
            EditorPrefs.SetInt("weaponTabSelected", value);
        }
    }

    private List<MonoBehaviour> _list;
    private GUIContent[] _editorNames;
    private readonly List<Tab> _tabs = new List<Tab>();
    
    private int _selectedTab;
    private bool _useMinified;

    static BaseWeaponWithExtensionsEditor()
    {
        var baseType = typeof(BaseWeaponExtension);
        ExtensionTypes = AllTypesContainer.AllTypes.FindAll(t => t.IsClass && t.IsSubclassOf(baseType));
        ExtensionTypes.Insert(0,null);
        ExtensionNames = EditorGUIUtility.TrTempContent(ExtensionTypes.Select(t => t!=null? t.Name:"(select)").ToArray());
        
    }

    private void OnEnable()
    {
        var target = this.target as BaseWeaponWithExtensions;
        _useMinified = EditorPrefs.GetBool("useWeaponMinifiedEditor", true);
        
        _tabs.Add(new Tab("Shooting"));
        _list = new List<MonoBehaviour>(target.extensions);
        for (var i = 0; i < _list.Count; i++)
        {
            var type = _list[i].GetType();
            var attr = type.GetCustomAttribute<ToolBarDisplayGroup>();

            var editor = CreateEditor(_list[i]);
            if (attr == null)
            {
                var tab = new Tab(type.Name);
                tab.editors.Add(editor);
                _tabs.Add(tab);
            }
            else
            {
                int index = _tabs.FindIndex(t => t.name == attr.groupName);
                if(index > -1) _tabs[index].editors.Add(editor);
                else
                {
                    var tab = new Tab(attr.groupName);
                    tab.editors.Add(editor);
                    _tabs.Add(tab);
                }
            }
        }


        _editorNames = EditorGUIUtility.TrTempContent(_tabs.Select(l => l.name).ToArray());
   //     ArrayUtility.Insert(ref _editorNames,0,new GUIContent("Shooting"));
        
        _selectedTab = Mathf.Clamp(EditorPrefs.GetInt("weaponTabSelected"),0,_tabs.Count);
        
        if(_useMinified) Minify();
        else UnMinify();
    }

    public override void OnInspectorGUI()
    {
        GUI.backgroundColor = new Color(1f, 0.58f, 0f, 0.21f);
        using (new GUILayout.HorizontalScope())
        {
            var content = new GUIContent("View Type: ");
            GUILayout.Label(content,GUILayout.Width(EditorStyles.label.CalcSize(content).x));
            EditorGUI.BeginChangeCheck();
            

            var viewType = GUILayout.Toolbar(_useMinified ? 0 : 1, new[] {"Designer", "Developer"},GUI.skin.button,GUI.ToolbarButtonSize.FitToContents);
            if (EditorGUI.EndChangeCheck())
            {
                _useMinified = viewType == 0;
                if(_useMinified) Minify();
                else UnMinify();
                InspectorUtility.RepaintGameView();
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.Space(9f);


        if (!_useMinified)
        {
            base.OnInspectorGUI();
            return;
        }

        for (int i = 0; i < _editorNames.Length; i+=3)
        {
            var filtered = _editorNames.Where((_, ind) => ind >= i && ind < i + 3).ToArray();
            int selectIndex = _selectedTab >= i && _selectedTab < i+3? _selectedTab - i:-1;
            EditorGUI.BeginChangeCheck();
            selectIndex = GUILayout.Toolbar(selectIndex, filtered,GUI.skin.button,GUI.ToolbarButtonSize.Fixed,GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                SelectedTab = i + selectIndex;
            }
        }
        //SelectedTab = GUILayout.Toolbar(_selectedTab, 
        //    _editorNames,GUI.skin.button,GUI.ToolbarButtonSize.FitToContents,GUILayout.ExpandHeight(true));
        
        if (_selectedTab == 0)
        {
            DrawPropertiesExcluding(serializedObject,"m_Script");
            foreach (var editor in _tabs[_selectedTab].editors)
            {
                GUILayout.Space(18f);
                editor.OnInspectorGUI();
            }
        }
        else
        {
            foreach (var editor in _tabs[_selectedTab].editors)
            {
                editor.OnInspectorGUI();
                GUILayout.Space(18f);
            }
        }

        DrawExtensionsAdd();
    }

    private void DrawExtensionsAdd()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Extensions", EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
        rect = EditorGUI.PrefixLabel(rect, EditorGUIUtility.TrTextContent("Add Extension"));

        int selection = EditorGUI.Popup(rect, 0, ExtensionNames);
        if (selection > 0)
        {
            var extType = ExtensionTypes[selection];
            bool disallowMultiple = extType.GetCustomAttribute<DisallowMultipleComponent>() != null;
            
            var targetGameObject = (target as BaseWeaponWithExtensions).gameObject;
            
            if (!targetGameObject) return;
            if (disallowMultiple)
            {
                if(!targetGameObject.GetComponent(extType))
                    Undo.AddComponent(targetGameObject, extType);
            }
            else
                Undo.AddComponent(targetGameObject, extType);
        }
    }
    private void Minify()
    {
        EditorPrefs.SetBool("useWeaponMinifiedEditor", true);

        foreach (var monoBehaviour in _list)
            monoBehaviour.hideFlags = HideFlags.HideInInspector;
        
    }
    
    
    private void UnMinify()
    {
        EditorPrefs.SetBool("useWeaponMinifiedEditor", false);

        foreach (var monoBehaviour in _list)
            monoBehaviour.hideFlags = HideFlags.None;
    }
}