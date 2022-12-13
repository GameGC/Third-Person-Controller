using System;
using System.Collections.Generic;
using ThirdPersonController.Core.DI.CustomEditor;
using UnityEngine;

namespace ThirdPersonController.Core.DI
{
    public class ReferenceResolver : MonoBehaviour , IReferenceResolver
    {
        [SerializeField,ComponentSelect] private List<Component> cachedComponents;
        [SerializeField] private Transform cameraTransform;

        [SerializeField] private bool releaseMemoryOnStart = true;


        private Transform _playerTransform;

        private void OnValidate()
        {
            if (cachedComponents == null || cachedComponents.Count < 1)
                cachedComponents = new List<Component>(new Component[] {transform});
        }

        private void Start()
        {
            if (!releaseMemoryOnStart) return;
            cachedComponents.Clear();
            cachedComponents = null;
        }

        public new T GetComponent<T>() where T: Component
        {
            if (cachedComponents.Count > 0)
            {
                foreach (var component in cachedComponents)
                {
                    if (component is T casted)
                        return casted;
                }

                var newComponent = base.GetComponent<T>();
                cachedComponents.Add(newComponent);
                return newComponent;
            }
            else
            {
                var component = base.GetComponent<T>();
                cachedComponents.Add(component);
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

      //public Transform GetPlayerTransform()
      //{
      //    return _playerTransform??= transform;
      //}

      //public BaseInputReader GetInput()
      //{
      //    return input;
      //}

        public Transform GetCamera() => cameraTransform;
    }
}