#if UNITY_EDITOR

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
    }
    
    
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
}

#endif
