using ThirdPersonController.Input;
using UnityEngine;

namespace StateMachineLogic.DI
{
    public interface IReferenceResolver
    {
        public T GetComponent<T>() where T: Component;

      // public Transform GetPlayerTransform();
      // 
      // public BaseInputReader GetInput();
        public Transform GetCamera();
    }
}