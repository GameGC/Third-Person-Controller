using System;
using System.Collections.Generic;
using System.Linq;
using GameGC.CommonEditorUtils.Editor;
using TypeNamespaceTree;
using UnityEditor;
using UnityEngine;

public partial class FuzzyWindow : EditorWindow
{
    private CategoryTree BuildTreeHierarchy(List<Type> types,bool isFeature)
    {
        var root = new CategoryTree("");
        var categorys = isFeature?FuzzyGroupsPreset.instance.GetFeatureTrees(root) : FuzzyGroupsPreset.instance.GetTransitionTrees(root);
        
        foreach (var c in categorys)
        {
            if (string.IsNullOrEmpty(c.Namespace))
            {
                var tree = new OptionTreeBuilder(types, c);
                tree.BuildTree(c.Namespace);
                c.Childs = tree.Root.Childs;
            }
            else
            {
                var ctypes = types.Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.Contains(c.Namespace));

                var tree = new OptionTreeBuilder(ctypes, c);
                tree.BuildTree(c.Namespace);
                c.Childs = tree.Root.Childs;

                types.RemoveAll(t => ctypes.Contains(t));
            }
        }

        root.Childs = new List<IOptionTree>(categorys.Where(c=>c.Childs.Count > 0).ToList());

        return root;
    }

    private IOptionTree[] BuildTree1d(List<Type> types)
    {
        var root = new CategoryTree("") {Content = EditorGUIUtility.TrTextContent("Search")};

        var optionsArray = new TypeTree[types.Count];
        for (int i = 0; i < types.Count; i++)
        {
            optionsArray[i] = new TypeTree(types[i]) {Parent = root};
        }

        return optionsArray;
    }

    private List<Type> GetNonAbstractTypesSubclassOf(Type parentType, bool sorted = true)
    {
        List<Type> types = AllTypesContainer.AllTypes
            .FindAll(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(parentType));

        if (sorted) types.Sort(CompareTypesNames);

        return types;
    }

    private static int CompareTypesNames(Type a, Type b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal);
}

public partial class FuzzyWindow : EditorWindow
{
    private void UpdateAnimation(in bool isRepaint)
    {
        if (_anim < 1)
        {
            OnLevelGUI(_anim + 1,in isRepaint);
        }

        if (isRepaint)
        {
            if (IsAnimating)
            {
                _anim = Mathf.MoveTowards(_anim, _animTarget, RepaintDeltaTime * _animationSpeed);

                if (_animTarget == 0 && _anim == 0)
                {
                    _anim = 1;
                    _animTarget = 1;
                }

                _list.Repaint = true;
            }

            _lastRepaintTime = DateTime.Now;
        }
    }
    
    private void EnterParent()
    {
        _animTarget = 0;
        _lastRepaintTime = DateTime.Now;
    }

    private void EnterChild()
    {
        _lastRepaintTime = DateTime.Now;
        _list.Repaint = true;

        if (_animTarget == 0)
        {
            _animTarget = 1;
        }
        else if (_anim == 1)
        {
            _anim = 0;
        }
    }
}