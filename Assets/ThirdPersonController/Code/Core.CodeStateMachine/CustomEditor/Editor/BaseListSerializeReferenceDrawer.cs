using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


[InitializeOnLoad]
public class BaseListSerializeReferenceDrawer
{
    private bool IsFirstArrayElement(SerializedProperty property)
    {
        var path = property.propertyPath;
        var index0 = path.LastIndexOf('[');

        try
        {
            var currentIndex = int.Parse(path.Substring(index0 + 1, path.Length - 2 - index0));
            return currentIndex < 1;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private SerializedProperty GetTargetProperty(SerializedProperty property)
    {
        var separated = property.propertyPath.Split('.');
        var target = separated[^3];

        if (separated.Length - 3 > 0)
        {
            var fullPathAdd = "";
            for (int i = 0; i < separated.Length - 3; i++)
            {
                fullPathAdd += separated[i] + '.';
            }

            target = fullPathAdd + target;
        }

        return property.serializedObject.FindProperty(target);
    }

    private delegate IAddButtonAttribute GetAttributesForField(SerializedProperty property);
    private static GetAttributesForField GetAttributesForFieldF;


    private static readonly List<Type> _targetTypesToInject;
    private static Dictionary<Type,object> _drawersImplementations;
    
    private List<Component> _selectionComponentList;



    static BaseListSerializeReferenceDrawer()
    {
        var unityEditorCoreModule =
            Assembly.Load("UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        var scriptAttributeUtil = unityEditorCoreModule.GetType("UnityEditor.ScriptAttributeUtility");
        var getFiledInfoMethod = scriptAttributeUtil.GetMethod("GetFieldInfoAndStaticTypeFromProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        var interface_ = typeof(IAddButtonAttribute);
        GetAttributesForFieldF = new GetAttributesForField(property =>
        {
            Type t = null;
            var filedInfo = getFiledInfoMethod.Invoke(null, new object[] {property, t}) as FieldInfo;
            return (IAddButtonAttribute) filedInfo?.GetCustomAttributes().FirstOrDefault(a => interface_.IsInstanceOfType(a));
        });

        LoadAllDrawersClasses();
        

        var instance = Activator.CreateInstance(typeof(BaseListSerializeReferenceDrawer)) as BaseListSerializeReferenceDrawer;
        _targetTypesToInject = instance.GetFilterTypes();
        
        Selection.selectionChanged += instance.OnSelectionChanged;
        if (Selection.activeGameObject) instance.OnSelectionChanged();
    }

    #region Target Types Filter : Types with add attributes

    private List<Type> GetFilterTypes()
    {
        var monoBehType = typeof(MonoBehaviour);
        return AllTypesContainer.AllTypes.FindAll(t => t.IsClass && !t.IsAbstract &&
                                                       t.IsSubclassOf(monoBehType)).FindAll(HasAddButtonAttribute);
    }

    private bool HasAddButtonAttribute(Type t)
    {
        var interface_ = typeof(IReferenceAddButton);
        
        var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var fieldInfo in fields)
        {
            if(fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute<SerializeField>() == null) continue;
            if(fieldInfo.FieldType.IsSubclassOf(typeof(UnityEngine.Object))) continue;
            foreach (var customAttributeData in fieldInfo.GetCustomAttributesData())
            {
                if (interface_.IsAssignableFrom(customAttributeData.AttributeType))
                    return true;
            }
            
            var type = fieldInfo.FieldType;
            if (!FilterByAmName(type.Assembly)) continue;
            // Debug.Log(type.Assembly);
            if (HasAddButtonAttribute(type))
                return true;
        }

        var properties = t.GetProperties(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach (var propertyInfo in properties)
        {
            if(propertyInfo.PropertyType.IsSubclassOf(typeof(UnityEngine.Object))) continue;
            foreach (var customAttributeData in propertyInfo.GetCustomAttributesData())
            {
                if (interface_.IsAssignableFrom(customAttributeData.AttributeType))
                    return true;
            }
            
            var type = propertyInfo.PropertyType;
            if (!FilterByAmName(type.Assembly)) continue;
            if (HasAddButtonAttribute(type))
                return true;
        }

        return false;
    }
    
    private bool FilterByAmName(Assembly assembly)
    {
        string name = assembly.FullName;
        if(name.StartsWith("Unity")) return  false;
        if(name.StartsWith("System")) return false;
        if(name.StartsWith("mscorlib")) return false;
        if(name.StartsWith("Cinemachine")) return false;
       
        return true;
    }
    
    #endregion

    private static void LoadAllDrawersClasses()
    {
        var foundTypes = AllTypesContainer.AllTypes
            .FindAll(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<AddHandlerForAttribute>() != null);
        _drawersImplementations = new Dictionary<Type, object>(capacity: foundTypes.Count);
        
        foreach (var type in foundTypes)
        {
            var attribute = type.GetCustomAttribute<AddHandlerForAttribute>();
            _drawersImplementations.Add(attribute.TargetPropertyAttribute,Activator.CreateInstance(type));
        }
    }
    
    private void OnSelectionChanged()
    {
        UnityEditor.Editor.finishedDefaultHeaderGUI -= OnReload;

        if(!Selection.activeGameObject) return;
        var list = new List<Component>();
        Selection.activeGameObject.GetComponents(typeof(MonoBehaviour),list);

        _selectionComponentList = list.FindAll(component => _targetTypesToInject.Contains(component.GetType()));
        if(_selectionComponentList.Count > 0)
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnReload;
    }
    
    private void OnReload(UnityEditor.Editor _)
    {
        if (Event.current.type != EventType.Repaint) return;

        foreach (var component in _selectionComponentList)
        {
            using (var editor = new SerializedObject(component))
            {
                using (SerializedProperty iterator = editor.GetIterator().Copy())
                {
                    bool next = iterator.NextVisible(true);

                    while (next)
                    {
                        iterator.isExpanded = true;

                        if (iterator.isArray)
                        {
                            var attribute = GetAttributesForFieldF(iterator);
                            if (attribute != null)
                            {
                                InjectListFooter(iterator, attribute);
                            }
                        }

                        next = iterator.NextVisible(true);
                    }
                }
            }
        }
        UnityEditor.Editor.finishedDefaultHeaderGUI -= OnReload;
    }



    private void InjectListFooter(SerializedProperty property, IAddButtonAttribute attribute)
    {
        var resultID = ReorderableListWrapperRef.GetPropertyIdentifier(property);
        var listElementUnCasted = PropertyHandlerRef.s_reorderableLists[resultID];


        ReorderableListWrapperRef element;

        if (listElementUnCasted == null)
        {
            element = new ReorderableListWrapperRef(property,EditorGUIUtility.TrTextContent(property.displayName),true);
            var temp = PropertyHandlerRef.s_reorderableLists;
            temp.Add(resultID,element.originalInstance);
            PropertyHandlerRef.s_reorderableLists = temp;
        }
        else
        {
            element = new ReorderableListWrapperRef(listElementUnCasted);
        }


        var handlerType = _drawersImplementations[attribute.GetType()];
        if (handlerType is ListDropdownAddDrawer addDropdownDrawer)
        {
            addDropdownDrawer.OnAddItemFromDropdown = OnAddItemFromDropdown;
            element.m_ReorderableList.onAddDropdownCallback =
                (rect, list) => addDropdownDrawer.AddDropdown(rect, list, attribute as IReferenceAddButton);
        }
        else if (handlerType is ListAddDrawer addDrawer)
        {
            element.m_ReorderableList.onAddCallback = addDrawer.Add;
        }
    }

    private void OnAddItemFromDropdown(object obj)
    {
        var element = obj as Tuple<SerializedProperty, Type, int, ReorderableList, string>;

        var _tempList = element.Item1;
        if (_tempList.propertyPath != element.Item5) _tempList = _tempList.serializedObject.FindProperty(element.Item5);
        int last = element.Item3;

        _tempList.InsertArrayElementAtIndex(last);
        SerializedProperty lastProp = _tempList.GetArrayElementAtIndex(last);
        lastProp.managedReferenceValue = Activator.CreateInstance(element.Item2);

        _tempList.serializedObject.ApplyModifiedProperties();
    }
}