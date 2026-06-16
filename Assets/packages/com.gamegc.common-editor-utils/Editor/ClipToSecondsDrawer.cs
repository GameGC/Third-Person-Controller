using System.Collections.Generic;
using CommonEditorUtils.Editor;
using GameGC.CommonEditorUtils.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameGC.CommonEditorUtils.Editor
{
    [CustomPropertyDrawer(typeof(ClipToSecondsAttribute))]
    internal class ClipToSecondsDrawer : PropertyDrawer
    {
        private static Dictionary<string, AnimationClip> tempBuffer = new Dictionary<string, AnimationClip>(10);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float width = position.width;
            position.width = width * 0.6666667F;
            EditorGUI.PropertyField(position, property, label);
            position.x +=  position.width;
            position.width = width * 0.33333334F;
        
            var path = property.propertyPath;
            tempBuffer.TryGetValue(path, out var clip);


      
        
            EditorGUI.BeginChangeCheck();
            clip= EditorGUI.ObjectField(position, clip,typeof(AnimationClip),false) as AnimationClip;
            if (EditorGUI.EndChangeCheck())
            {
                if (!tempBuffer.ContainsKey(path))
                    tempBuffer.Add(path, clip);
                else tempBuffer[path] = clip;

                property.floatValue = clip.length;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = UIElementsExtras.HorizontalContainer;

            
            // Метка
            var label = UIElementsExtras.Label(property.displayName);
            container.Add(label);

            // Поле для float (секунд)
            var floatField = new FloatField();
            floatField.style.flexGrow = 1;
            floatField.value = property.floatValue;
            container.Add(floatField);

            // ObjectField для AnimationClip
            var clipField = new ObjectField()
            {
                objectType = typeof(AnimationClip),
                allowSceneObjects = false,
                style = { flexGrow = 1, marginLeft = 4 }
            };

            tempBuffer.TryGetValue(property.propertyPath, out var cachedClip);
            clipField.value = cachedClip;

            container.Add(clipField);

            // Связываем floatField с property
            floatField.RegisterValueChangedCallback(evt =>
            {
                property.floatValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            // Обработка изменения AnimationClip
            clipField.RegisterValueChangedCallback(evt =>
            {
                var newClip = evt.newValue as AnimationClip;
                if (!tempBuffer.ContainsKey(property.propertyPath))
                    tempBuffer.Add(property.propertyPath, newClip);
                else
                    tempBuffer[property.propertyPath] = newClip;

                if (newClip != null)
                {
                    property.floatValue = newClip.length;
                    floatField.SetValueWithoutNotify(newClip.length);
                }
                else
                {
                    property.floatValue = 0f;
                    floatField.SetValueWithoutNotify(0f);
                }
                property.serializedObject.ApplyModifiedProperties();
            });

            return container;
        }
    }
}