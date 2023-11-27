using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypeNamespaceTree;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class FuzzyWindowC2 : EditorWindow
{
    private CategoryTree BuildTreeHierarchy(List<Type> types)
    {
        var root = new CategoryTree("");
        var categorys = FuzzyGroupsPreset.instance.GetCategoryTrees(root);
        
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

        root.Childs = new List<IOptionTree>(categorys.ToList());

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
        List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(parentType))
            .ToList();

        if (sorted) types.Sort(CompareTypesNames);

        return types;
    }

    private static int CompareTypesNames(Type a, Type b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal);
}

public partial class FuzzyWindowC2 : EditorWindow
{
    private void UpdateAnimation(in bool isRepaint)
    {
        if (anim < 1)
        {
            OnLevelGUI(anim + 1,in isRepaint);
        }

        if (isRepaint)
        {
            if (isAnimating)
            {
                anim = Mathf.MoveTowards(anim, animTarget, repaintDeltaTime * animationSpeed);

                if (animTarget == 0 && anim == 0)
                {
                    anim = 1;
                    animTarget = 1;
                }

                _list.Repaint = true;
            }

            lastRepaintTime = DateTime.Now;
        }
    }
    
    private void EnterParent()
    {
        animTarget = 0;
        lastRepaintTime = DateTime.Now;
    }

    private void EnterChild()
    {
        lastRepaintTime = DateTime.Now;
        _list.Repaint = true;

        if (animTarget == 0)
        {
            animTarget = 1;
        }
        else if (anim == 1)
        {
            anim = 0;
        }
    }
}