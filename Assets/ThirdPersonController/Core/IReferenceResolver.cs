using System;
using UnityEngine;

namespace ThirdPersonController.Core.DI
{
    public interface IReferenceResolver
    {
        public bool isReady { get; }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// get component cached
        /// </summary>
        public T GetComponent<T>();
        
        /// <summary>
        /// get component cached
        /// </summary>
        public Component GetComponent(Type componentType);

        /// <summary>
        /// add component cached
        /// </summary>
        public void AddComponent<T>() where T : Component;
        
        /// <summary>
        /// add existing component cached
        /// </summary>
        public void AddCachedComponent<T>(T component) where T : Component;
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// get component cached
        /// </summary>
        public T GetNamedComponent<T>(string componentName) where T: Component;
        
        /// <summary>
        /// add component cached
        /// </summary>
        public T AddNamedComponent<T>(string componentName) where T: Component;

        /// <summary>
        /// add existing component cached
        /// </summary>
        public void AddNamedCachedComponent<T>(string componentName,T component) where T: Component;
    }
}