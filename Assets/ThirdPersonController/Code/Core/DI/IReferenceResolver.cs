using System;
using UnityEngine;

namespace ThirdPersonController.Core.DI
{
    public interface IReferenceResolver
    {
        public T GetComponent<T>() where T: Component;
        public Component GetComponent(Type componentType);

      // public Transform GetPlayerTransform();
      // 
      // public BaseInputReader GetInput();
        public Transform GetCamera();
    }
}