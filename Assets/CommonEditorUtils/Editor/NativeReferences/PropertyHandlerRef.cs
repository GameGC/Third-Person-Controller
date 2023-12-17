using System.Collections;
using System.Reflection;

public class PropertyHandlerRef
{
    private static readonly FieldInfo s_reorderableListsRef;

    static PropertyHandlerRef()
    {
        var unityEditorCoreModule = Assembly.Load("UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        var propertyHandler = unityEditorCoreModule.GetType("UnityEditor.PropertyHandler");
        s_reorderableListsRef = propertyHandler.GetField("s_reorderableLists", BindingFlags.Static | BindingFlags.NonPublic);
    }

    public static IDictionary s_reorderableLists
    {
        get => s_reorderableListsRef.GetValue(null) as IDictionary;
        set => s_reorderableListsRef.SetValue(null,value);
    }
}