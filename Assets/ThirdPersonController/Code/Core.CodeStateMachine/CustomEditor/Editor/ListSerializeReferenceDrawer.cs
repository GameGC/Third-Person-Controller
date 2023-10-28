#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor
{

 //   [CustomPropertyDrawer(typeof(SerializeReferenceAddButton))]
 [InitializeOnLoad]
 public class ListCustomSerializeButton : BaseListSerializeReferenceDrawer
    {
        //var resultID = method.Invoke(null, new[] {property});
        private static readonly MethodInfo GetPropertyIdentifier;
        private static readonly FieldInfo s_reorderableLists;
        private static readonly FieldInfo m_ReorderableList;

        private static Action forawrdToGui;

        private delegate SerializeReferenceAddButton GetAttributesForField(SerializedProperty property);

        private static GetAttributesForField GetAttributesForFieldF;
      //  private static Predicate<PropertyAttribute> GetAttributesForField(SerializedProperty property);
        static ListCustomSerializeButton()
        {
            var unityEditorCoreModule = Assembly.Load("UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            
            var reorderableListWrapper = unityEditorCoreModule.GetType("UnityEditorInternal.ReorderableListWrapper");
            //static
            GetPropertyIdentifier = reorderableListWrapper.GetMethod("GetPropertyIdentifier", BindingFlags.Static | BindingFlags.Public);
            //internal
            m_ReorderableList = reorderableListWrapper.GetField("m_ReorderableList", BindingFlags.Instance | BindingFlags.NonPublic);

            
            var propertyHandler = unityEditorCoreModule.GetType("UnityEditor.PropertyHandler");
            s_reorderableLists = propertyHandler.GetField("s_reorderableLists", BindingFlags.Static | BindingFlags.NonPublic);

            var scriptAttributeUtil = unityEditorCoreModule.GetType("UnityEditor.ScriptAttributeUtility");
            var getFiledInfoMethod = scriptAttributeUtil.GetMethod("GetFieldInfoAndStaticTypeFromProperty",BindingFlags.Static | BindingFlags.NonPublic);
            
            GetAttributesForFieldF = new GetAttributesForField(property =>
            {
                Type t = null;
                var filedInfo = getFiledInfoMethod.Invoke(null,new object[]{ property, t}) as FieldInfo;
               // return scriptAttributeUtil.GetMethod("GetFieldAttributes").Invoke(null, new []{filedInfo}) as List<PropertyAttribute>;
               return filedInfo.GetCustomAttribute<SerializeReferenceAddButton>();
            });

            


            var instance = new ListCustomSerializeButton();
            Selection.selectionChanged += instance.OnSelectionChanged;
            if(Selection.activeGameObject)
                instance.OnSelectionChanged();
            //EditorApplication.update += Update;
        }

        private void OnSelectionChanged()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI -= EditorOnfinishedDefaultHeaderGUI;
            
            if (!Selection.activeGameObject) return;
            if (!Selection.activeGameObject.TryGetComponent<StateMachine.CodeStateMachine>(out var st)) return;
            
            UnityEditor.Editor.finishedDefaultHeaderGUI += EditorOnfinishedDefaultHeaderGUI;
        }
        
        private void EditorOnfinishedDefaultHeaderGUI(UnityEditor.Editor obj)
        {
            if(Event.current.type != EventType.Layout && Event.current.type!= EventType.Repaint) return;
            
            if (!Selection.activeGameObject) return;
            if (!Selection.activeGameObject.TryGetComponent<StateMachine.CodeStateMachine>(out var st)) return;
            
            var editor =new SerializedObject(st);
            SerializedProperty iterator = editor.GetIterator();
            bool next = iterator.NextVisible(true);
        
            while (next)
            {
                bool couldDraw = DoesFitDrawCondition(iterator);
                if (couldDraw)
                {
                    var attribute = GetAttributesForFieldF(iterator);
                    if(attribute != null) 
                        DoListFooter(GetTargetProperty(iterator),attribute);
                }
                else
                {
                    if (iterator.name == "states")
                        DoStateListFooter(iterator);
                }
                next = iterator.NextVisible(true);
            }
        }
        
      //  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      //  {
      //      forawrdToGui?.Invoke();
//
      //      label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));
      //      EditorGUI.PropertyField(position, property,label,true);
      //      
      //      bool couldDraw = DoesFitDrawCondition(property); 
      //      if(!couldDraw) return;
//
      //      DoListFooter(GetTargetProperty(property));
//
      //  }


     
        
        private void DoListFooter(SerializedProperty property,SerializeReferenceAddButton attribute)
        {
            var resultID = GetPropertyIdentifier.Invoke(null, new[] {property});
            var s_reorderableListsValue = s_reorderableLists.GetValue(null) as IDictionary;


            var element = s_reorderableListsValue[resultID];
            
            if(element == null) return;
            
            var reorderableList = m_ReorderableList.GetValue(element) as ReorderableList;
            reorderableList.onAddDropdownCallback = (rect, list) => OnReorderListAddDropdown(rect,list,attribute);
        }

        public void DoStateListFooter(SerializedProperty property)
        {
            var resultID = GetPropertyIdentifier.Invoke(null, new[] {property});
            var s_reorderableListsValue = s_reorderableLists.GetValue(null) as IDictionary;


            var element = s_reorderableListsValue[resultID];
            
            if(element == null) return;
            
            var reorderableList = m_ReorderableList.GetValue(element) as ReorderableList;
            reorderableList.onAddCallback = list =>
            {

                int index = list.selectedIndices.Count > 0
                    ? list.selectedIndices[0]
                    : list.count - 1;

                list.ClearSelection();

                
                var stateMachine = list.serializedProperty.GetPropertyParent<StateMachine.CodeStateMachine>();
                ref var states = ref stateMachine.states;

                var copy = new State(states[index]);
                copy.Name += " (1)";
                ArrayUtility.Insert(ref states,index+1,copy);
                list.serializedProperty.serializedObject.Update();
                stateMachine.OnValidate();
                return;
                //var copy = list.serializedProperty.GetArrayElementAtIndex(index).Copy();
                //var path = copy.propertyPath.Replace($"[{index}]", $"[{index+1}]");
                //index++;
                //
                //var  featuresProp=   copy.FindPropertyRelative("features");
                //for (int i = 0; i < featuresProp.arraySize; i++)
                //{
                //    var pathProperty = featuresProp.GetArrayElementAtIndex(i).FindPropertyRelative("path");
                //    pathProperty.stringValue = pathProperty.stringValue.Replace(copy.propertyPath, path);
                //}
                //list.serializedProperty.arraySize++;
              //  list.serializedProperty.GetArrayElementAtIndex(index).obj = copy.managedReferenceValue;
            };
        }


        private void OnReorderListAddDropdown(Rect buttonRect, ReorderableList list,SerializeReferenceAddButton attribute)
        {
            var menu = new GenericMenu();

            List<Type> showTypes = GetNonAbstractTypesSubclassOf(attribute.baseType);

            foreach (var type in showTypes)
            {
                string actionName = type.Name;

                // UX improvement: If no elements are available the add button should be faded out or
                // just not visible.
                //bool alreadyHasIt = DoesReordListHaveElementOfType(actionName,list);
                //if (alreadyHasIt)
                //    continue;

                InsertSpaceBeforeCaps(ref actionName);
                var dynamicType =
                    new Tuple<SerializedProperty, Type, int, ReorderableList,string>(list.serializedProperty, type, list.count,
                        list,list.serializedProperty.propertyPath);
                menu.AddItem(new GUIContent(actionName), false, OnAddItemFromDropdown, dynamicType);
                
            }

            menu.ShowAsContext();
        }

        private void OnAddItemFromDropdown(object obj)
        {
            var element = obj as Tuple<SerializedProperty, Type, int, ReorderableList,string>;

            var _tempList = element.Item1;

            int last = element.Item3;

            try
            {
                _tempList.InsertArrayElementAtIndex(last);
                SerializedProperty lastProp = _tempList.GetArrayElementAtIndex(last);
                lastProp.managedReferenceValue = Activator.CreateInstance(element.Item2);

                _tempList.serializedObject.ApplyModifiedProperties();
            }
            catch
            {
                forawrdToGui = () =>
                {
                    Debug.Log("invoke");
                    var element = obj as Tuple<SerializedProperty, Type, int, ReorderableList,string>;

                    var _tempList =element.Item1.serializedObject.FindProperty(element.Item5);

                    int last = element.Item3;

                        _tempList.InsertArrayElementAtIndex(last);
                        SerializedProperty lastProp = _tempList.GetArrayElementAtIndex(last);
                        lastProp.managedReferenceValue = Activator.CreateInstance(element.Item2);

                        _tempList.serializedObject.ApplyModifiedProperties();
                    forawrdToGui = null;
                };
            }



         
        }
        
        
        
        #region Helper Methods

       private void InsertSpaceBeforeCaps(ref string theString)
        {
            for (int i = 0; i < theString.Length; ++i)
            {
                char currChar = theString[i];

                if (char.IsUpper(currChar))
                {
                    theString = theString.Insert(i, " ");
                    ++i;
                }
            }
        }

        private List<Type> GetNonAbstractTypesSubclassOf(Type parentType,bool sorted = true)
        {
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a=>a.GetTypes()).Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(parentType)).ToList();

            if (sorted)
                types.Sort(CompareTypesNames);

            return types;
        }
    

        private int CompareTypesNames(Type a, Type b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal);

        private bool DoesReordListHaveElementOfType(string type,ReorderableList list)
        {
            for (int i = 0; i < list.serializedProperty.arraySize; ++i)
            {
                // this works but feels ugly. Type in the array element looks like "managedReference<actualStringType>"
                if (list.serializedProperty.GetArrayElementAtIndex(i).type.Contains(type))
                    return true;
            }

            return false;
        }
        #endregion
    }
 
 [CustomPropertyDrawer(typeof(BaseFeature),true)]
 public class BaseFeatureDrawer : PropertyDrawer
 {
     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
     {
         return EditorGUI.GetPropertyHeight(property,label,true);
     }

     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
     {
         property.FindPropertyRelative("path").stringValue = property.propertyPath;
         label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));
         EditorGUI.PropertyField(position, property,label,true);
     }
     
     private string GetPropertyTypeName(SerializedProperty property)
     {
         string actionName = property.managedReferenceFullTypename.Split(" ").Last();

         var split = actionName.Split('.');
         if (split.Length > 2) 
             actionName = split[^2] + '.' + split[^1];

         return actionName;
     }
 }
 
 [CustomPropertyDrawer(typeof(BaseStateTransition),true)]
 public class BaseTransitionDrawer : PropertyDrawer
 {
     public static bool hideDestinationState;
     
     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
     {
         return EditorGUI.GetPropertyHeight(property,label,true)
                + (hideDestinationState && property.isExpanded? - EditorGUIUtility.singleLineHeight : 0);
     }

     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
     {
         label = EditorGUIUtility.TrTextContent(GetPropertyTypeName(property));
         if(!hideDestinationState) EditorGUI.PropertyField(position, property,label,true);
         else
         {
             Rect temp = position;
             temp.height = EditorGUIUtility.singleLineHeight;
             position.height -= temp.height;
             position.y += temp.height;
             
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
     
     private string GetPropertyTypeName(SerializedProperty property)
     {
         string actionName = property.managedReferenceFullTypename.Split(" ").Last();

         var split = actionName.Split('.');
         if (split.Length > 2) 
             actionName = split[^2] + '.' + split[^1];

         return actionName;
     }
 }
 
public static class GCUtils
{
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
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
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
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }

        return obj;
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

#endif
