using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TypeNamespaceTree
{
    public class CategoryTree : IOptionTree
    {
        public string Namespace { get; set; }
        public List<IOptionTree> Childs { get; set; }
        public CategoryTree Parent { get; set; }

        public CategoryTree()
        {
            Childs = new List<IOptionTree>();
        }
        public CategoryTree(string ns)
        {
            Namespace = ns;
            Childs = new List<IOptionTree>();
            Content = EditorGUIUtility.TrTextContent(Namespace);
        }

        public GUIContent Content { get; set; }
    }

    public class TypeTree : IOptionTree
    {
        public Type Type { get; set; }
        public string TypeName { get; set; }

        public GUIContent Content { get; }
        public CategoryTree Parent { get; set; }

        public TypeTree(Type type)
        {
            Type = type;
            TypeName = type.Name;
            Content = EditorGUIUtility.TrTextContent(TypeName);
        }

    }

    public interface IOptionTree
    {
        public GUIContent Content { get; }

        public CategoryTree Parent { get; set; }
    }

    public class OptionTreeBuilder
    {
        private IEnumerable<Type> Types { get; set; }
        public CategoryTree Root { get; set; }

        public OptionTreeBuilder(IEnumerable<Type> types,CategoryTree Root)
        {
            Types = types;
            this.Root = Root;
        }

        public void BuildTree(string rootNamespace = null)
        {
            foreach (Type type in Types)
            {
                string fullNamespace = type.Namespace;
                if (!string.IsNullOrEmpty(rootNamespace))
                {
                    fullNamespace = type.Namespace.Replace(rootNamespace, "");
                    if (fullNamespace.StartsWith('.'))
                        fullNamespace = fullNamespace.Remove(0, 1);
                }
                
                if (!string.IsNullOrWhiteSpace(fullNamespace))
                {
                    string[] namespaces = fullNamespace.Split('.');

                    CategoryTree currentNode = Root;
                    foreach (string ns in namespaces)
                    {
                        CategoryTree existingNode = (CategoryTree) 
                            currentNode.Childs.Find(node => node is CategoryTree tree && tree.Namespace == ns);
                        
                        if (existingNode != null)
                        {
                            currentNode = existingNode;
                        }
                        else
                        {
                            CategoryTree newNode = new CategoryTree(ns)
                            {
                                Parent = currentNode
                            };
                            currentNode.Childs.Add(newNode);
                            currentNode.Childs= currentNode.Childs.OrderByDescending(c=> c is TypeTree).ToList();

                            currentNode = newNode;
                        }
                    }

                    currentNode.Childs.Add(new TypeTree(type)
                    {
                        Parent = currentNode
                    });
                    currentNode.Childs= currentNode.Childs.OrderByDescending(c=> c is TypeTree).ToList();
                }
                else
                {
                    Root.Childs.Add(new TypeTree(type)
                    {
                        Parent = Root
                    });
                    Root.Childs= Root.Childs.OrderByDescending(c=> c is TypeTree).ToList();
                }
            }
        }
    }
}