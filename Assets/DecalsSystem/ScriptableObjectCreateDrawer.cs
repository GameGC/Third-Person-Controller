using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameGC.Collections;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(ScriptableObjectCreateAttribute))]
public class ScriptableObjectCreateDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property,label);
    }

    private Object target;
    private Type _type;
    private string drawingPath;
    public ScriptableObjectCreateDrawer()
    {
        EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
    }
    ~ScriptableObjectCreateDrawer()
    {
        EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attribute = this.attribute as ScriptableObjectCreateAttribute;
        GetCollectionBaseType(attribute.BaseType ?? fieldInfo.FieldType);
        drawingPath = property.propertyPath;
      
        target = property.serializedObject.targetObject;
        EditorGUI.PropertyField(position, property, label);
    }

    private void GetCollectionBaseType(Type baseType)
    {
        _type = baseType;
        if(_type.IsGenericType && _type.GetGenericTypeDefinition()
           == typeof(List<>))
        {
            _type = _type.GetGenericArguments()[0];
        }
        else if(_type.IsArray)
        {
            _type = _type.GetElementType();
        }
    }
    private void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
    {
        if (property.serializedObject.targetObject != target) return;
        if (drawingPath == property.propertyPath)
        {
            if (property.objectReferenceValue == null)
            {
                string propertyPath = property.propertyPath;
                var turple = new STurple<string, Object, int>(propertyPath, target, 0);

                if (!_type.IsAbstract)
                {
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Create {_type.Name} Nested"), false, CreateNested,turple);
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Create {_type.Name} Global"), false, CreateGlobal,turple);
                }
            
                var typeCopy = _type;
                foreach (var inheritedClass in GetInheritedClasses(_type))
                {
                    _type = inheritedClass;
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Create {inheritedClass.Name} Nested"), false, CreateNested,turple);
                    menu.AddItem(EditorGUIUtility.TrTextContent($"Create {inheritedClass.Name} Global"), false, CreateGlobal,turple);
                }

                _type = typeCopy;
            }
            else
            {
                var copy = property.Copy();
                if (AssetDatabase.IsSubAsset(copy.objectReferenceValue))
                {
                    menu.AddItem(new GUIContent("UnParent"), false, ()=>
                    {
                        var parentObj = AssetDatabase.GetAssetPath(copy.serializedObject.targetObject);
                        var path = Path.GetPathRoot(AssetDatabase.GetAssetPath(Selection.activeObject));
                        path += $"/{_type.Name}.asset";
                        var clone = copy.objectReferenceValue;
                        ProjectWindowUtil.CreateAsset(clone,path);
               
                        AssetDatabase.RemoveObjectFromAsset(copy.objectReferenceValue);
                        AssetDatabase.ImportAsset(parentObj,ImportAssetOptions.ForceUpdate);
                    });
                }
                else
                {
                    menu.AddItem(new GUIContent("Parent"), false, ()=>
                    {
                        var parentObj = AssetDatabase.GetAssetPath(copy.serializedObject.targetObject);

                        var path = AssetDatabase.GetAssetPath(copy.objectReferenceValue);
                        AssetDatabase.RemoveObjectFromAsset(copy.objectReferenceValue);
                        AssetDatabase.AddObjectToAsset(copy.objectReferenceValue,parentObj);
                        AssetDatabase.ImportAsset(parentObj,ImportAssetOptions.ForceUpdate);
                        AssetDatabase.DeleteAsset(path);
                    });
                }
            }
        }
    }

    private static STurple<string, Object,int> lastKv;

    private void CreateNested(object data)
    {
        var kv = ( STurple<string, Object,int>) data;
        var propertyPath = kv.Item1;
      
        var asset = ScriptableObject.CreateInstance(_type);
        var index = int.Parse(propertyPath.Substring(propertyPath.IndexOf('[') + 1, 
            propertyPath.IndexOf(']') - propertyPath.IndexOf('[') - 1));

        asset.name = $"{Enum.GetNames(typeof(SurfaceHitType))[index]}Effect";

        AssetDatabase.AddObjectToAsset(asset, Selection.activeObject);
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Selection.activeObject),ImportAssetOptions.ForceUpdate);
      
        var propertyCopy = new SerializedObject(kv.Item2).FindProperty(kv.Item1);
        propertyCopy.objectReferenceInstanceIDValue = asset.GetInstanceID();
      
        propertyCopy.serializedObject.ApplyModifiedProperties();
        propertyCopy.serializedObject.Dispose();
    }
   
    private void CreateGlobal(object data)
    {
        var asset = ScriptableObject.CreateInstance(_type);
        var instanceID = asset.GetInstanceID();
      
        lastKv = ( STurple<string, Object,int>) data;
        lastKv.Item3 = instanceID;

        var path = Path.GetPathRoot(AssetDatabase.GetAssetPath(Selection.activeObject));
        path += $"/{_type.Name}.asset";


        ProjectWindowUtil.CreateAsset(asset, path);
        EditorApplication.projectChanged += ProjectWindowChanged;
    }

    private void ProjectWindowChanged()
    {
        var propertyCopy = new SerializedObject(lastKv.Item2).FindProperty(lastKv.Item1);
        propertyCopy.objectReferenceInstanceIDValue = lastKv.Item3;
      
        propertyCopy.serializedObject.ApplyModifiedProperties();
        propertyCopy.serializedObject.Dispose();
        EditorApplication.projectChanged -= ProjectWindowChanged;
    }


    private IEnumerable<Type> GetInheritedClasses(Type MyType)
    {
        //if you want the abstract classes drop the !TheType.IsAbstract but it is probably to instance so its a good idea to keep it.
        return Assembly.GetAssembly(MyType).GetTypes().Where(TheType => TheType.IsClass && !TheType.IsAbstract 
            && (TheType.IsSubclassOf(MyType)|| TheType.GetInterfaces().Contains(MyType)));
    }
}