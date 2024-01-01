using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor
{
    public static class GCUIElementsUtils
    {
        private static PropertyInfo inspectorModeField = typeof(UnityEditor.Editor).GetProperty("inspectorMode",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        /// <summary>
        ///        <para>
        /// Adds default inspector property fields under a container VisualElement
        /// </para>
        ///      </summary>
        /// <param name="container">The parent VisualElement</param>
        /// <param name="serializedObject">The SerializedObject to inspect</param>
        /// <param name="editor">The editor currently used</param>
        public static void FillDefaultInspectorWithExclude(
            VisualElement container,
            SerializedObject serializedObject,
            UnityEditor.Editor editor,params string[] excludeNames)
        {
            if (serializedObject == null)
                return;
            bool isPartOfPrefabInstance = (UnityEngine.Object) editor != (UnityEngine.Object) null &&
                                          IsAnyMonoBehaviourTargetPartOfPrefabInstance(editor);
            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                
                var inspectorMode = (InspectorMode) inspectorModeField.GetValue(editor);

                
                do
                {
                    if(Array.IndexOf(excludeNames,iterator.name) > -1) continue;
                    
                    PropertyField propertyField = new PropertyField(iterator)
                    {
                        name = "PropertyField:" + iterator.propertyPath
                    };
                    PropertyField child = propertyField;
                    
                    if (iterator.propertyPath == "m_Script" &&
                        (!((UnityEngine.Object) editor != (UnityEngine.Object) null) ||
                         inspectorMode != InspectorMode.Debug &&
                         inspectorMode != InspectorMode.DebugInternal) &&
                        ((serializedObject.targetObject != (UnityEngine.Object) null
                             ? 1
                             : (iterator.objectReferenceValue != (UnityEngine.Object) null ? 1 : 0)) |
                         (isPartOfPrefabInstance ? 1 : 0)) != 0)
                        
                        
                        
                        child.SetEnabled(false);
                    container.Add((VisualElement) child);
                }
                while (iterator.NextVisible(false));
            }
        }
        
        public static void FillDefaultInspectorWithIncludeByCount(
            VisualElement container,
            SerializedObject serializedObject,
            UnityEditor.Editor editor,int count)
        {
            if (serializedObject == null)
                return;
            bool isPartOfPrefabInstance = (UnityEngine.Object) editor != (UnityEngine.Object) null &&
                                          IsAnyMonoBehaviourTargetPartOfPrefabInstance(editor);
            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                
                var inspectorMode = (InspectorMode) inspectorModeField.GetValue(editor);

                int i = 0;
                do
                {
                    PropertyField propertyField = new PropertyField(iterator)
                    {
                        name = "PropertyField:" + iterator.propertyPath
                    };
                    PropertyField child = propertyField;
                    
                    if (iterator.propertyPath == "m_Script" &&
                        (!((UnityEngine.Object) editor != (UnityEngine.Object) null) ||
                         inspectorMode != InspectorMode.Debug &&
                         inspectorMode != InspectorMode.DebugInternal) &&
                        ((serializedObject.targetObject != (UnityEngine.Object) null
                             ? 1
                             : (iterator.objectReferenceValue != (UnityEngine.Object) null ? 1 : 0)) |
                         (isPartOfPrefabInstance ? 1 : 0)) != 0)
                        
                        
                        
                        child.SetEnabled(false);
                    container.Add((VisualElement) child);
                    i++;
                }
                while (iterator.NextVisible(false) && i < count);
            }
        }

        
        private static bool IsAnyMonoBehaviourTargetPartOfPrefabInstance(UnityEditor.Editor editor)
        {
            if (!(editor.target is MonoBehaviour))
                return false;
            foreach (UnityEngine.Object target in editor.targets)
            {
                if (PrefabUtility.IsPartOfNonAssetPrefabInstance(target))
                    return true;
            }
            return false;
        }
    }
    public static class GCUtils
    {
        public static int CountChildProperties(this SerializedProperty property)
        {
            var tempProperty = property.serializedObject.FindProperty(property.propertyPath);

            bool wasExpanded = tempProperty.isExpanded;
            tempProperty.isExpanded = true;
            int propertyCount = tempProperty.CountInProperty();
            property.isExpanded = wasExpanded;

            return propertyCount;
        }

        public static bool IsParentExpanded(this SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                return property.serializedObject.FindProperty(element.Replace("[", ".Array.data[")).isExpanded;
            }

            throw new NotImplementedException();
        }

        public static void DrawSmallPropertyOneLine2Labels(SerializedProperty property, Rect temp, GUIContent label,
            bool maximumMinify = true)
        {
            float label0Width = maximumMinify ? EditorStyles.label.CalcSize(label).x : EditorGUIUtility.labelWidth;

            GUIContent label1Content = EditorGUIUtility.TrTextContent(property.displayName, property.tooltip);
            float label1Width = EditorStyles.label.CalcSize(label1Content).x;

            temp.height = EditorGUI.GetPropertyHeight(property);
            float maxWidth = temp.width;
            temp.width = label0Width;
            EditorGUI.LabelField(temp, label);
            temp.x += 10f + temp.width;
            temp.width = maxWidth - temp.width - 10f;

            float prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth =
                temp.width > prevLabelWidth * 1.5f ? EditorGUIUtility.labelWidth : label1Width;
            EditorGUI.PropertyField(temp, property, label1Content);
            EditorGUIUtility.labelWidth = prevLabelWidth;
        }

        public static T GetProperty<T>(this SerializedProperty prop) => (T) GetProperty(prop);

        public static object GetProperty(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("["))
                        .Replace("[", "")
                        .Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }

        public static T GetPropertyParent<T>(this SerializedProperty prop) => (T) GetPropertyParent(prop);

        public static object GetPropertyParent(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("["))
                        .Replace("[", "")
                        .Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }
        
        public static SerializedProperty GetParentProperty(this SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            if (path.EndsWith(']'))
                return property.serializedObject.FindProperty(path.Substring(0, path.LastIndexOf('['))
                    .Replace("[", ".Array.data["));

            throw new NotImplementedException();
            Debug.Log(path);
            var elements = path.Split('.');
            if (elements == null || elements.Length <2)
            {
                return property.serializedObject.FindProperty(path.Substring(0,path.IndexOf('[')));
            }

            foreach (var element in elements.Take(elements.Length - 1))
            {
                Debug.Log(element.Replace("[", ".Array.data["));
                return property.serializedObject.FindProperty(element.Replace("[", ".Array.data["));
            }

            throw new NotImplementedException();
        }

        private static object GetValue(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) return null;
                return p.GetValue(source, null);
            }

            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0) enm.MoveNext();
            return enm.Current;
        }
    }
}