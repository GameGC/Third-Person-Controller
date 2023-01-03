using System.Collections.Generic;
using System.Linq;
using GameGC.Collections;
using GameGC.Collections.Editor;
using UnityEditor;
using UnityEngine;
using static ThirdPersonController.Core.DI.CustomEditor.Editor.ComponentSelectDrawer;

namespace ThirdPersonController.Core.DI.CustomEditor.Editor
{
    [CustomPropertyDrawer(typeof(ComponentSelectAttribute))]
    internal sealed class ComponentSelectDrawer : PropertyDrawerWithCustomData<DataContainer>
    {
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
                EditorGUI.PropertyField(position, property, label);
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
    }

    [CustomPropertyDrawer(typeof(SKeyValuePair<string, Component>))]
    internal class StringComponentKeyValDrawer : PropertyDrawer
    {
        static readonly SKeyValuePairDrawer _baseDrawer = new SKeyValuePairDrawer();
        static readonly ComponentSelectDrawer _componentGUIDrawer = new ComponentSelectDrawer();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _baseDrawer.TryDrawLabel(ref position,property,label);
            _baseDrawer.OnKeyGUI(property,position);
            
            
            var valueProp = property.FindPropertyRelative("Value");
            
            var valuePos = position;
            valuePos.width /= 2;
            valuePos.x += valuePos.width;
            valuePos.height = EditorGUI.GetPropertyHeight(valueProp);
            
            _componentGUIDrawer.OnGUI(valuePos,valueProp,GUIContent.none);
        }
    }
}