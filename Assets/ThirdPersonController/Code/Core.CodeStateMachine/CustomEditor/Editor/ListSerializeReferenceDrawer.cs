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
                    DoListFooter(GetTargetProperty(iterator));
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


        private string GetPropertyTypeName(SerializedProperty property)
        {
            string actionName = property.managedReferenceFullTypename.Split(" ").Last();

            var split = actionName.Split('.');
            if (split.Length > 2)
            {
                actionName = split[^2] + '.' + split[^1];
            }

            return actionName;
        }
        
        private void DoListFooter(SerializedProperty property)
        {
            var resultID = GetPropertyIdentifier.Invoke(null, new[] {property});
            var s_reorderableListsValue = s_reorderableLists.GetValue(null) as IDictionary;


            var element = s_reorderableListsValue[resultID];
            
            if(element == null) return;
            
            var reorderableList = m_ReorderableList.GetValue(element) as ReorderableList;
            reorderableList.onAddDropdownCallback = OnReorderListAddDropdown;
        }
        
        
        private void OnReorderListAddDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            // var typeName = list.serializedProperty.GetArrayElementAtIndex(0).managedReferenceFullTypename;
            // var split = typeName.Split(" ");
            // 
            // var asm = Assembly.Load(split[0]);
            // var _type = asm.GetType(split[1]).BaseType;

          //  var attrInfo = (SerializeReferenceAddButton) attribute;
            List<Type> showTypes = GetNonAbstractTypesSubclassOf(typeof(BaseFeature));

            foreach (var type in showTypes)
            {
                string actionName = type.Name;

                // UX improvement: If no elements are available the add button should be faded out or
                // just not visible.
                bool alreadyHasIt = DoesReordListHaveElementOfType(actionName,list);
                if (alreadyHasIt)
                    continue;

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
    

        private int CompareTypesNames(Type a, Type b)
        {
            return a.Name.CompareTo(b.Name);
        }

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
    
    
    
    
   
    /*
    internal sealed class ListSerializeReferenceDrawer : BaseListSerializeReferenceDrawer
    {
    
        #region Private Attributes

        private const float AdditionalSpaceMultiplier = 1.0f;

        private const float heightHeader = 20.0f;
        private const float shrinkHeaderWidth = 15.0f;
        private const float xShiftHeaders = 15.0f;


        #endregion

        private static readonly Dictionary<string,ReorderableList> CachedLists = new Dictionary<string, ReorderableList>();
        private ReorderableList _tempList;
        #region Editor Methods


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            try
            {
                bool couldDraw = DoesFitDrawCondition(property);

                if (!couldDraw) return 0;

     
                if (!CachedLists.TryGetValue(property.propertyPath, out _tempList))
                    return EditorGUI.GetPropertyHeight(GetTargetProperty(property),
                        EditorGUIUtility.TrTempContent(property.displayName));
                else
                    return _tempList.GetHeight();
            }
            catch (Exception e)
            {
                CachedLists.Clear();
                return GetPropertyHeight(property, label);
            }

            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool couldDraw = DoesFitDrawCondition(property);

            if (!couldDraw) return;
        
            var path = property.propertyPath;
            
            if (!CachedLists.TryGetValue(path,out _tempList))
            {
                //isCachced = true;
                property = GetTargetProperty(property);
                var serializedObject = property.serializedObject;

                _tempList = new ReorderableList(serializedObject, property, true, false, true, true)
                {
                    drawElementCallback = OnDrawReorderListElement,
                    elementHeightCallback = OnReorderListElementHeight,
                    onAddDropdownCallback = OnReorderListAddDropdown,
                };
                CachedLists.Add(path,_tempList);
            }
        
            _tempList.DoList(EditorGUI.IndentedRect(position));
        }

        #endregion

        #region ReorderableList Callbacks


        private void OnDrawReorderListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            ReorderableList.defaultBehaviours.DrawElement(rect, _tempList.serializedProperty.GetArrayElementAtIndex(index), _tempList.serializedProperty.GetArrayElementAtIndex(index),isActive,isFocused,true,true);

            return;
            int length = _tempList.serializedProperty.arraySize;

            if (length <= 0)
                return;

            SerializedProperty iteratorProp = _tempList.serializedProperty.GetArrayElementAtIndex(index);

            string actionName = iteratorProp.managedReferenceFullTypename.Split(" ").Last();
        
            {
                var split = actionName.Split('.');
                if (split.Length > 2)
                {
                    actionName = split[^2] +'.'+ split[^1];
                }
            }


            Rect labelfoldRect = rect;
            labelfoldRect.height = heightHeader;
            labelfoldRect.x += xShiftHeaders;
            labelfoldRect.width -= shrinkHeaderWidth;

            iteratorProp.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(labelfoldRect, iteratorProp.isExpanded, actionName);

            if (iteratorProp.isExpanded)
            {
                ++EditorGUI.indentLevel;

                SerializedProperty endProp = iteratorProp.GetEndProperty();

                iteratorProp.NextVisible(true);
            
                int i = 0;
                while (!EqualContents(endProp, iteratorProp))
                {
                    float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
                    rect.y += GetDefaultSpaceBetweenElements(i>0?rect.height:0) * multiplier;
                    rect.height = EditorGUI.GetPropertyHeight(iteratorProp);

                    EditorGUI.PropertyField(rect, iteratorProp, false);

                    ++i;
                    iteratorProp.NextVisible(false);
                }

                --EditorGUI.indentLevel;
            }

            EditorGUI.EndFoldoutHeaderGroup();
        }

    
        private float OnReorderListElementHeight(int index)
        {
            int length = _tempList.serializedProperty.arraySize;

            if (length <= 0)
                return 0.0f;

            SerializedProperty iteratorProp = _tempList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty endProp = iteratorProp.GetEndProperty();

            float height = GetDefaultSpaceBetweenElements();

            if (!iteratorProp.isExpanded)
                return height;

            int i = 0;
            bool next = iteratorProp.NextVisible(true);
        
            while (next && !EqualContents(endProp, iteratorProp))
            {
                float multiplier = i == 0 ? AdditionalSpaceMultiplier : 1.0f;
                height += GetDefaultSpaceBetweenElements() * multiplier;
                next = iteratorProp.NextVisible(false);
                ++i;
            }

            return height;
        }

        private void OnReorderListAddDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            // var typeName = list.serializedProperty.GetArrayElementAtIndex(0).managedReferenceFullTypename;
            // var split = typeName.Split(" ");
            // 
            // var asm = Assembly.Load(split[0]);
            // var _type = asm.GetType(split[1]).BaseType;

            var attrInfo = (ListOfSerializeReference) attribute;
            List<Type> showTypes = GetNonAbstractTypesSubclassOf(attrInfo.baseType);

            foreach (var type in showTypes)
            {
                string actionName = type.Name;

                // UX improvement: If no elements are available the add button should be faded out or
                // just not visible.
                bool alreadyHasIt = DoesReordListHaveElementOfType(actionName,list);
                if (alreadyHasIt)
                    continue;

                InsertSpaceBeforeCaps(ref actionName);
                menu.AddItem(new GUIContent(actionName), false, OnAddItemFromDropdown, (object)type);
            }

            menu.ShowAsContext();
        }

        private void OnAddItemFromDropdown(object obj)
        {
            Type settingsType = (Type)obj;

            int last = _tempList.serializedProperty.arraySize;
            _tempList.serializedProperty.InsertArrayElementAtIndex(last);

            SerializedProperty lastProp = _tempList.serializedProperty.GetArrayElementAtIndex(last);
            lastProp.managedReferenceValue = Activator.CreateInstance(settingsType);

            _tempList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Helper Methods

        private float GetDefaultSpaceBetweenElements(float previousHeight = 0)
        {
            return (previousHeight>0?previousHeight:EditorGUIUtility.singleLineHeight) + EditorGUIUtility.standardVerticalSpacing;
        }

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

        private bool EqualContents(SerializedProperty a, SerializedProperty b)
        {
            return SerializedProperty.EqualContents(a, b);
        }

        private List<Type> GetNonAbstractTypesSubclassOf(Type parentType,bool sorted = true)
        {
            Assembly assembly = Assembly.GetAssembly(parentType);

            List<Type> types = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(parentType)).ToList();

            if (sorted)
                types.Sort(CompareTypesNames);

            return types;
        }
    

        private int CompareTypesNames(Type a, Type b)
        {
            return a.Name.CompareTo(b.Name);
        }

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
    }*/
}

#endif
