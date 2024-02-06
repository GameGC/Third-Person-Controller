using System;
using System.Collections.Generic;
using System.Linq;
using GameGC.Collections;
using GameGC.CommonEditorUtils.Attributes;
using UnityEditor;
using UnityEngine;

namespace ThirdPersonController.Core.DI
{
    [DisallowMultipleComponent]
    public class ReferenceResolver : MonoBehaviour , IReferenceResolver
    {
        [SerializeField,ComponentSelect] private List<Component> cachedComponents;
        [SerializeField] private bool releaseMemoryOnStart = true;


        [SerializeField] private SDictionary<string, Component> namedReferences;
        private void OnValidate()
        {
            if (cachedComponents == null || cachedComponents.Count < 1)
                cachedComponents = new List<Component>(new Component[] {transform});
        }

        private void Start()
        {
            isReady = true;
            if (!releaseMemoryOnStart) return;
            cachedComponents.Clear();
            cachedComponents = null;
        }

        public bool isReady { get; set; } = false;

        public new T GetComponent<T>()
        {
            if (cachedComponents.Count > 0)
            {
                foreach (var component in cachedComponents)
                {
                    if (component is T casted)
                        return casted;
                }

                var newComponent = base.GetComponent<T>();
                cachedComponents.Add(newComponent as Component);
                return newComponent;
            }
            else
            {
                var component = base.GetComponent<T>();
                cachedComponents.Add(component as Component);
                return component;
            }
        }

        public new Component GetComponent(Type componentType)
        {
            if (cachedComponents.Count > 0)
            {
                foreach (var component in cachedComponents)
                {
                    var mType = component.GetType();
                    if (mType == componentType || componentType.IsSubclassOf(mType) || mType.IsSubclassOf(componentType))
                        return component;
                }

                var newComponent = base.GetComponent(componentType);
                cachedComponents.Add(newComponent);
                return newComponent;
            }
            else
            {
                var component = base.GetComponent(componentType);
                cachedComponents.Add(component);
                return component;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public T AddComponent<T>() where T : Component
        {
            var component = gameObject.AddComponent<T>();
            cachedComponents.Add(component);
            return component;
        }

        public void AddCachedComponent<T>(T component)  where T : Component
        {
            if (cachedComponents.Count > 0)
            {
                for (var i = 0; i < cachedComponents.Count; i++)
                {
                    var tempComponent = cachedComponents[i];
                    if (tempComponent is T)
                    {
                        cachedComponents[i] = component;
                        return;
                    }
                }

                cachedComponents.Add(component);
            }
            else
            {
                cachedComponents.Add(component);
            }
        }

        

        public T GetNamedComponent<T>(string componentName) where T : Component
        {
            foreach (var (key, component) in namedReferences)
            {
                if (key == componentName && component is T casted) return casted;
            }

            Debug.LogError(new NullReferenceException("No component with such key:" + componentName));
            return null;
        }

        public T AddNamedComponent<T>(string componentName) where T : Component
        {
            var component = gameObject.AddComponent<T>();
            namedReferences.Add(componentName, component);
            return component;
        }
        
        public void AddNamedCachedComponent<T>(string componentName ,T component) where T : Component
        {
            if (namedReferences.TryGetValue(componentName,out _))
            {
                namedReferences[componentName] = component;
            }
            else namedReferences.Add(componentName,component);
        }


#if UNITY_EDITOR
        [ContextMenu("Remap")]
        public void Editor_Remap()
        {
            for (var i = 0; i < cachedComponents.Count; i++)
            {
                var cachedComponent = cachedComponents[i];
                    var newComponent = gameObject.GetComponent(cachedComponent.GetType());
                    if (newComponent)
                        cachedComponents[i] = newComponent;
            }

            var keys = namedReferences.Keys.ToArray();
            for (int i = 0; i < namedReferences.Count; i++)
            {
                var value = namedReferences[keys[i]];
                if(!value) continue;
                if(!value.transform.parent) return;
                var type = value.GetType();

                var newTransform = transform.Find(AnimationUtility.CalculateTransformPath(value.transform,
                    value.transform.root));
                if(newTransform)
                    namedReferences[keys[i]] = newTransform.GetComponent(type);
                
            }
        }
#endif

    }

    [UnityEditor.CustomEditor(typeof(ReferenceResolver))]
    public class ReferenceResolverEditor : Editor
    {
        public override void OnInspectorGUI() => DrawDefaultInspector();
    }
}