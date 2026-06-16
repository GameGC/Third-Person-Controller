#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using CommonEditorUtils.Editor;
using GameGC.Collections;
using GameGC.CommonEditorUtils.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor
{
    
    [CustomPropertyDrawer(typeof(ComponentSelectAttribute))]
    internal sealed class ComponentSelectDrawer : PropertyDrawer
    {
        /*
        public class DataContainer
        {
            public bool _isCached = false;
            public GUIContent[] _componentsNames = null;
            public List<Component> _components = new List<Component>();
            public int _selected = 0;
        }

        protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label, DataContainer customData)
        {
            if (property.objectReferenceValue)
            {
                EditorGUIUtility.labelWidth /= 2;
                
                if (!customData._isCached)
                {
                    var component = property.objectReferenceValue as Component;
                    if (component)
                    {
                        component.GetComponents(customData._components);
                        customData._componentsNames = EditorGUIUtility.TrTempContent(customData._components.Select(s => s.GetType().Name).ToArray());

                        customData._selected = customData._components.IndexOf(component);
                    }

                    customData._isCached = true;
                }
            
                EditorGUI.BeginChangeCheck();
                position.width /= 2;
                EditorGUI.PropertyField(position, property,GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorGUIUtility.labelWidth *= 2;
                    customData._isCached = false;
                    return;
                }

                position.x += position.width;
            
                EditorGUI.BeginChangeCheck();
                customData._selected = EditorGUI.Popup(position, customData._selected,customData._componentsNames);
                if (EditorGUI.EndChangeCheck())
                {
                    property.objectReferenceValue = customData._components[customData._selected];
                }
                
                
                EditorGUIUtility.labelWidth *= 2;
            }
            else
            {
                customData._isCached = false;
                EditorGUI.PropertyField(position, property, label);
            }
        }
        */
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = UIElementsExtras.HorizontalContainer;

            var label = UIElementsExtras.HalfLabel(property.displayName);
            container.Add(label);

            var objectField = new ObjectField {objectType = typeof(Component), value = property.objectReferenceValue};
            objectField.style.flexGrow = 0;
            objectField.style.flexShrink = 0;
            objectField.style.flexBasis = new Length(50, LengthUnit.Percent);
            container.Add(objectField);

            var popup = new PopupField<string>(new List<string>(), 0);
            popup.style.flexGrow = 1;
            popup.style.flexShrink = 0;
            container.Add(popup);

            var components = new List<Component>();
            var componentNames = new List<string>();
            int selectedIndex = 0;

            void UpdateComponents(Component comp)
            {
                components.Clear();
                if (comp != null)
                {
                    comp.GetComponents(components);
                    componentNames = components.Select(c => c.GetType().Name).ToList();
                    selectedIndex = Mathf.Max(components.IndexOf(comp), 0);
                }
                else
                {
                    componentNames.Clear();
                    selectedIndex = 0;
                }
            }

            if (property.objectReferenceValue != null) 
                UpdateComponents(property.objectReferenceValue as Component);

            popup.choices = componentNames;
            popup.index = selectedIndex;

            objectField.RegisterValueChangedCallback(evt =>
            {
                var newComp = evt.newValue as Component;
                property.objectReferenceValue = newComp;
                property.serializedObject.ApplyModifiedProperties();

                UpdateComponents(newComp);

                popup.choices = componentNames;
                popup.index = selectedIndex;
            });

            popup.RegisterValueChangedCallback(evt =>
            {
                int newIndex = popup.index;
                if (newIndex >= 0 && newIndex < components.Count)
                {
                    property.objectReferenceValue = components[newIndex];
                    property.serializedObject.ApplyModifiedProperties();

                    objectField.value = components[newIndex];
                }
            });

            if (property.objectReferenceValue == null)
            {
                objectField.BindProperty(property);
                popup.choices = new List<string>();
                popup.index = 0;
            }

            return container;
        }
    }
}

#endif

/*
    [CustomPropertyDrawer(typeof(SKeyValuePair<string,Component>))]
    public class SKeyValuePairComponentDrawer : SKeyValuePairDrawer
    {
        public override void OnValueGUI(SerializedProperty property, Rect position)
        {
            var valueProp = property.FindPropertyRelative("Value");
            var component = valueProp.objectReferenceValue as Component;
            if (component)
            {
                EditorGUIUtility.labelWidth /= 2;
                
                List<Component> _components = new List<Component>();
                component.GetComponents(_components);
                var _componentsNames = EditorGUIUtility.TrTempContent(_components.Select(s => s.GetType().Name).ToArray());

                var _selected = _components.IndexOf(component);
                
                
                
                EditorGUI.BeginChangeCheck();
                position.width /= 3;
                position.x += position.width;

                EditorGUI.PropertyField(position, valueProp,GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorGUIUtility.labelWidth *= 2;
                    return;
                }
                position.x += position.width;
            
                EditorGUI.BeginChangeCheck();
                _selected = EditorGUI.Popup(position, _selected,_componentsNames);
                if (EditorGUI.EndChangeCheck())
                {
                    valueProp.objectReferenceValue = _components[_selected];
                }
                
                
                EditorGUIUtility.labelWidth *= 2;
            }
        }
    }
*/