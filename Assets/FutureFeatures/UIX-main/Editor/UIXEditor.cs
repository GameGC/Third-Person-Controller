using System;
using RedOwl.UIX.Engine;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedOwl.UIX.Editor
{
    public static class UIXEditor
    {
        public static void Setup<T>(T instance, VisualElement root) where T : EditorWindow
        {
            Type type = instance.GetType();
            root.AddToClassList("RedOwl");
            root.AddToClassList("UIEX");
            root.AddToClassList(type.Namespace);
            root.AddToClassList(type.Name);
            
            HandleUXMLAttribute(instance, root);
            HandleUSSAttributes(instance, root);
            HandleUSSClassAttributes(instance, root);
            HandleUXMLElementAttributes(instance, root);
            HandleQueryAttributes(instance, root);
            RegisterCallbacks(instance, root);
        }

        public static void Setup<T>(T instance) where T : VisualElement
        {
            var type = instance.GetType();
            instance.name = type.Name;
            instance.AddToClassList("RedOwl");
            instance.AddToClassList("UIEX");
            instance.AddToClassList(type.Namespace);
            instance.AddToClassList(type.Name);
            HandleUXMLAttribute(instance, instance);
            HandleUSSAttributes(instance, instance);
            HandleUSSClassAttributes(instance, instance);
            HandleUXMLElementAttributes(instance, instance);
            HandleQueryAttributes(instance, instance);
            RegisterCallbacks(instance, instance);
        }

        public static T Show<T>() where T : EditorWindow
        {
            var instance = EditorWindow.GetWindow<T>();
            instance.WithAttr<T, WindowTitleAttribute>((attr) =>
            {
                instance.titleContent = new GUIContent(attr.Title);
            });
            instance.WithAttr<T, WindowMinSizeAttribute>((attr) =>
            {
                instance.minSize = new Vector2(attr.Width, attr.Height);
            });
            instance.WithAttr<T, WindowMaxSizeAttribute>((attr) =>
            {
                instance.maxSize = new Vector2(attr.Width, attr.Height);
            }, false);
            return instance;
        }
        
        public static void HandleUXMLAttribute<T>(T instance, VisualElement root)
        {
            instance.WithAttr<T, UXMLAttribute>((attr) =>
            {
                var visualTree = attr.Load();
                if (visualTree != null)
                {
                    var element = visualTree.CloneTree();
                    if (element != null) root.Add(element);
                }
            }, false);
        }
        
        public static void HandleUSSAttributes<T>(T instance, VisualElement root)
        {
            instance.GetType().WithAttr<USSAttribute>((attr) =>
            {
                var styleSheet = attr.Load();
                if (styleSheet != null) root.styleSheets.Add(styleSheet);
            }, true);
        }
        
        private static void HandleUSSClassAttributes<T>(T instance, VisualElement element)
        {
            instance.WithAttr<T, USSClassAttribute>((attr) => 
            {
                foreach (string name in attr.Names)
                {
                    Debug.LogFormat($"Adding USSClass '{name}' to '{instance.GetType().FullName}'");
                    element.AddToClassList(name);
                }
            }, true);
        }

        public static void HandleUXMLElementAttributes<T>(T instance, VisualElement element)
        {
            instance.ForFieldWithAttr<T, UXMLElementAttribute>((attr, info) => 
            {
                string uxmlName = attr.GetName(info.Name);
                Debug.LogFormat($"Populating 'UXMLElement' field on '{instance.GetType().FullName}.{info.Name}' by looking for UXML name '{uxmlName}'");
                info.SetValue(instance, element.Q(uxmlName));
            }, true);
        }
        
        public static void HandleQueryAttributes<T>(T instance, VisualElement element)
        {
            instance.ForMethodWithAttr<T, QAttribute>((attr, info) => 
            {
                //Debug.LogFormat("Registering 'QAttribute' on '{0}.{1}'", instance.GetType().Name, info.Name);
                info.Invoke(instance, new object[] {element.Query(attr.GetName(info.Name), attr.GetClasses()).First()});
            }, false);
        }
        
        private static void RegisterCallbacks<T>(T instance, VisualElement element)
        {
            instance.ForMethodWithAttr<T, CallbackAttribute>((attr, method) =>
            {
                var parameters = method.GetParameters();
                if (parameters.Length < 1) return;
                var type = parameters[0].ParameterType;
                
                // TODO: Add More OR Figure out more generic way - https://docs.unity3d.com/Manual/UIE-Events-Reference.html
                if (type == typeof(GeometryChangedEvent))
                {
                    var callback = (EventCallback<GeometryChangedEvent>) Delegate.CreateDelegate(typeof(EventCallback<GeometryChangedEvent>), instance, method);
                    element.RegisterCallback(callback);
                }
                if (type == typeof(FocusEvent))
                {
                    var callback = (EventCallback<FocusEvent>) Delegate.CreateDelegate(typeof(EventCallback<FocusEvent>), instance, method);
                    element.RegisterCallback(callback);
                }
                if (type == typeof(FocusOutEvent))
                {
                    var callback = (EventCallback<FocusOutEvent>) Delegate.CreateDelegate(typeof(EventCallback<FocusOutEvent>), instance, method);
                    element.RegisterCallback(callback);
                }
                if (type == typeof(MouseDownEvent))
                {
                    var callback = (EventCallback<MouseDownEvent>) Delegate.CreateDelegate(typeof(EventCallback<MouseDownEvent>), instance, method);
                    element.RegisterCallback(callback);
                }
                if (type == typeof(MouseUpEvent))
                {
                    var callback = (EventCallback<MouseUpEvent>) Delegate.CreateDelegate(typeof(EventCallback<MouseUpEvent>), instance, method);
                    element.RegisterCallback(callback);
                }
                if (type == typeof(KeyDownEvent))
                {
                    var callback = (EventCallback<KeyDownEvent>) Delegate.CreateDelegate(typeof(EventCallback<KeyDownEvent>), instance, method);
                    element.RegisterCallback(callback);
                }
                if (type == typeof(KeyUpEvent))
                {
                    var callback = (EventCallback<KeyUpEvent>) Delegate.CreateDelegate(typeof(EventCallback<KeyUpEvent>), instance, method);
                    element.RegisterCallback(callback);
                }
            });
            
            instance.ForMethodWithAttr<T, ScheduleAttribute>((attr, info) => 
            {
                if (attr.Once)
                {
                    //Debug.LogFormat("Adding Callback '{0}' to '{1}' once after {2} milliseconds", info, instance.GetType().Name, attr.Interval);
                    element.schedule.Execute(() => {info.Invoke(instance, null);}).StartingIn(attr.Interval);
                } else {
                    //Debug.LogFormat("Adding Callback '{0}' to '{1}' every {2} milliseconds", info, instance.GetType().Name, attr.Interval);
                    element.schedule.Execute(() => {info.Invoke(instance, null);}).Every(attr.Interval).Resume();
                }
            }, false);
        }

        public static void DrawInto<T>(T instance, VisualElement element)
        {
            foreach (var info in instance.GetType().GetFields())
            {
                var field = new UIXBuilder().For(instance, info).WithLabel(ObjectNames.NicifyVariableName(info.Name));
                if (field != null) element.Add(field);
            }
            // TODO: GetProperties with "ShowInInspector" or "field: SerializeField" and CreateField for them
            // TODO: GetMethods with "Button" and CreateButton for them - if method has args then create fields for them
        }
    }
}