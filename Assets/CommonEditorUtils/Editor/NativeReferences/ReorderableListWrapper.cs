using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ReorderableListWrapperRef
{
    private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    
    private static Type reorderableListWrapper;

    private static FieldInfo m_ReorderableListRef;
    private static PropertyInfo PropertyRef;
    private static MethodInfo GetPropertyIdentifierRef;
    private static MethodInfo InitRef;
    private static MethodInfo GetHeightRef;
    
    private static MethodInfo DrawRef;
    private static MethodInfo DrawChildredRef;
    
    public object originalInstance;
    
    static ReorderableListWrapperRef()
    {
        var unityEditorCoreModule = Assembly.Load("UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        reorderableListWrapper = unityEditorCoreModule.GetType("UnityEditorInternal.ReorderableListWrapper");

        m_ReorderableListRef = reorderableListWrapper.GetField("m_ReorderableList", InstanceFlags);
        PropertyRef = reorderableListWrapper.GetProperty("Property",InstanceFlags);
        GetPropertyIdentifierRef = reorderableListWrapper.GetMethod("GetPropertyIdentifier",StaticFlags);
        InitRef = reorderableListWrapper.GetMethod("Init",InstanceFlags);
        GetHeightRef = reorderableListWrapper.GetMethod("GetHeight",InstanceFlags);
        
        DrawRef = reorderableListWrapper.GetMethod("Draw",InstanceFlags);
        DrawChildredRef = reorderableListWrapper.GetMethod("DrawChildren",InstanceFlags);
    }

    public ReorderableList m_ReorderableList;


    public SerializedProperty Property
    {
        get => PropertyRef.GetValue(originalInstance) as SerializedProperty;
        set => PropertyRef.SetValue(originalInstance,value);
    }

    public static string GetPropertyIdentifier(SerializedProperty serializedProperty)
    {
        return (string) GetPropertyIdentifierRef.Invoke(null,new object[]{serializedProperty});
    }

    public ReorderableListWrapperRef()
    {
        originalInstance = Activator.CreateInstance(reorderableListWrapper);
        m_ReorderableList = m_ReorderableListRef.GetValue(originalInstance) as ReorderableList;
    }
    public ReorderableListWrapperRef(object originalInstance)
    {
        this.originalInstance = originalInstance;
        m_ReorderableList = m_ReorderableListRef.GetValue(originalInstance) as ReorderableList;
    }

    public ReorderableListWrapperRef(SerializedProperty property, GUIContent label, bool reorderable = true)
    {
        var empty = Type.EmptyTypes;
        var constructor = reorderableListWrapper.GetConstructor(BindingFlags.NonPublic|BindingFlags.Instance, null, empty, null);
        originalInstance =constructor.Invoke(null);
        Init(reorderable, property);
        m_ReorderableList = m_ReorderableListRef.GetValue(originalInstance) as ReorderableList;
    }

    private void Init(bool reorderable, SerializedProperty property)
    {
        InitRef.Invoke(originalInstance, new object[] {reorderable, property});
    }

    public float GetHeight() => (float) GetHeightRef.Invoke(originalInstance, null);

    public void Draw(
        GUIContent label,
        Rect r,
        Rect visibleArea,
        string tooltip,
        bool includeChildren)
    {
        DrawRef.Invoke(originalInstance, new object[] {label,r,visibleArea,tooltip,includeChildren });
    }

    public 
        void DrawChildren(
            Rect listRect,
            Rect headerRect,
            Rect sizeRect,
            Rect visibleRect,
            EventType previousEvent)
    {
        DrawChildredRef.Invoke(originalInstance, new object[] {listRect,headerRect,sizeRect,visibleRect,previousEvent});
    }
}