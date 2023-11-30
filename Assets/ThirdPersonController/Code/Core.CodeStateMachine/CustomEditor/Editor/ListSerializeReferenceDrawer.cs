using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor
{
    public abstract class BaseCodeStateMachineDrawer<T> : BaseListSerializeReferenceDrawer<T> where T : Attribute, IReferenceAddButton
    {
        protected override void OnReload(UnityEditor.Editor obj)
        {
            if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) return;

            if (!Selection.activeGameObject) return;
            if (!Selection.activeGameObject.TryGetComponent<StateMachine.CodeStateMachine>(out var st)) return;

            var editor = new SerializedObject(st);
            SerializedProperty iterator = editor.GetIterator();
            bool next = iterator.NextVisible(true);

            while (next)
            {
                bool couldDraw = IsFirstArrayElement(iterator);

                if (couldDraw)
                {
                    var attribute = GetAttributesForFieldF(iterator);
                    if (attribute != null) DoListFooter(GetTargetProperty(iterator), attribute);
                }
                else if (iterator.name == "states") DoStateListFooter(iterator);

                next = iterator.NextVisible(true);
            }
        }

        private void DoStateListFooter(SerializedProperty property)
        {
            var resultID = ReorderableListWrapperRef.GetPropertyIdentifier(property);
            var listElementUnCasted = PropertyHandlerRef.s_reorderableLists[resultID];

            if (listElementUnCasted == null) return;
            ReorderableListWrapperRef element = new ReorderableListWrapperRef(listElementUnCasted);

            var reorderableList = element.m_ReorderableList;
            reorderableList.onAddCallback = list =>
            {
                if (reorderableList.count > 0)
                {
                    int index = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : list.count - 1;

                    list.ClearSelection();

                    var stateMachine = list.serializedProperty.GetPropertyParent<StateMachine.CodeStateMachine>();
                    ref var states = ref stateMachine.states;

                    var copy = new State(states[index]);
                    copy.Name += " (1)";
                    ArrayUtility.Insert(ref states, index + 1, copy);
                    list.serializedProperty.serializedObject.Update();
                    stateMachine.OnValidate();
                }
                else
                {
                    ReorderableList.defaultBehaviours.DoAddButton(list);
                }
            };
        }
    }
    
    [InitializeOnLoad]
    public class ListCustomSerializeButton : BaseCodeStateMachineDrawer<SerializeReferenceAddButton>
    {
        static ListCustomSerializeButton()
        {
            Init(typeof(ListCustomSerializeButton));
        }
    }

    [CustomPropertyDrawer(typeof(BaseFeature), true)]
    public class BaseFeatureDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.CountChildProperties() < 3) return EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.FindPropertyRelative("path") == null) return;
            property.FindPropertyRelative("path").stringValue = property.propertyPath;
            label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));

            //Debug.Log(label.text+" "+property.CountChildProperties());
            int childsCount = property.CountChildProperties();
            if (childsCount < 3)
            {
                if (childsCount > 1)
                {
                    property.NextVisible(true);
                    GCUtils.DrawSmallPropertyOneLine2Labels(property, position, label, false);
                }
                else
                {
                    EditorGUI.LabelField(position, label);
                }

                return;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        private string GetPropertyTypeName(SerializedProperty property)
        {
            string actionName = property.managedReferenceFullTypename.Split(" ").Last();

            var split = actionName.Split('.');
            if (split.Length > 2) actionName = split[^2] + '.' + split[^1];

            return actionName;
        }
    }

    [CustomPropertyDrawer(typeof(BaseStateTransition), true)]
    public class BaseTransitionDrawer : PropertyDrawer
    {
        public static bool hideDestinationState;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            if (!property.IsParentExpanded()) return height;

            int propertyCount = property.CountChildProperties();
            ;

            if (hideDestinationState)
            {
                if (propertyCount < 4) return EditorGUIUtility.singleLineHeight;
                if (property.isExpanded) height -= EditorGUIUtility.singleLineHeight;
            }
            else
            {
                if (propertyCount < 3) return EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!property.IsParentExpanded()) return;
            label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));

            string initPath = property.propertyPath;
            int propertyCount = property.CountChildProperties();

            if (!hideDestinationState)
            {
                if (propertyCount < 3)
                {
                    //EditorGUI.LabelField(property,label);
                    DrawSmallProperty(property, position, label, initPath, false);
                }
                else
                    EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                Rect temp = position;
                temp.height = EditorGUIUtility.singleLineHeight;
                position.height -= temp.height;
                position.y += temp.height;

                if (propertyCount < 4)
                {
                    if (propertyCount < 3)
                        EditorGUI.LabelField(temp, label);
                    else
                        DrawSmallProperty(property, temp, label, initPath);
                    return;
                }

                EditorGUI.PropertyField(temp, property, label, false);
                if (property.isExpanded)
                {
                    string basePath = property.propertyPath;

                    property.NextVisible(true);
                    if (property.name != "_transitionName" && property.name != "_transitionIndex")
                    {
                        temp = position;
                        temp.height = EditorGUI.GetPropertyHeight(property);
                        EditorGUI.PropertyField(temp, property, false);
                        position.height -= temp.height;
                        position.y += temp.height;
                    }

                    //pCount--;
                    while (property.NextVisible(false) && property.propertyPath.StartsWith(basePath))
                    {
                        if (property.name != "_transitionName" && property.name != "_transitionIndex")
                        {
                            temp = position;
                            temp.height = EditorGUI.GetPropertyHeight(property);
                            EditorGUI.PropertyField(temp, property, false);
                            position.height -= temp.height;
                            position.y += temp.height;
                        }
                    }
                }
            }
        }

        private void DrawSmallProperty(SerializedProperty property, Rect temp, GUIContent label, string initPath,
            bool hideDestination = true)
        {
            property.NextVisible(true);
            if (!hideDestinationState)
            {
                GCUtils.DrawSmallPropertyOneLine2Labels(property, temp, label);
                return;
            }

            while (property.NextVisible(false) && property.propertyPath.StartsWith(initPath))
            {
                if (property.name is not ("_transitionName" or "_transitionIndex"))
                {
                    GCUtils.DrawSmallPropertyOneLine2Labels(property, temp, label);
                    return;
                }
            }
        }

        private string GetPropertyTypeName(SerializedProperty property)
        {
            string actionName = property.managedReferenceFullTypename.Split(" ").Last();

            var split = actionName.Split('.');
            if (split.Length > 2) actionName = split[^2] + '.' + split[^1];

            return actionName;
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
            var elements = path.Split('.');
            if (elements == null || elements.Length <2)
            {
                return property.serializedObject.FindProperty(path.Substring(0,path.IndexOf('[')));
            }

            Debug.Log(path);
            Debug.Log(elements[0]);
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