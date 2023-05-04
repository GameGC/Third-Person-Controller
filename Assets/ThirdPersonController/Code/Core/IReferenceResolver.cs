using System;
using UnityEngine;

namespace ThirdPersonController.Core.DI
{
    public interface IReferenceResolver
    {
        public bool isReady { get; }
        
        public T GetComponent<T>();
        
        public T GetNamedComponent<T>(string name) where T: Component;
        
        public Component GetComponent(Type componentType);

      // public Transform GetPlayerTransform();
      // 
      // public BaseInputReader GetInput();
        public Transform GetCamera();
        
        
    }
}