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
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// get component cached
        /// </summary>
        public T GetNamedComponent<T>(string name) where T: Component;
        
        /// <summary>
        /// get component cached
        /// </summary>
        public Component GetComponent(Type componentType);

      
        public Transform GetCamera();
        
        
    }
}