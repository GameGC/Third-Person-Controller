using System;
using ThirdPersonController.Core.DI;
using UnityEngine;

namespace ThirdPersonController.Core
{
    [Serializable]
    public abstract class BaseFeature
    {
        
        public abstract void CacheReferences(IStateMachineVariables variables,IReferenceResolver resolver);
    
        public virtual void OnEnterState(){}
        public virtual void OnUpdateState(){}
        public virtual void OnFixedUpdateState(){}
        public virtual void OnExitState(){}

        // there issues in unity editor ,so this is a fix
#if UNITY_EDITOR

        [HideInInspector]
        public string path;

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
        
#endif
        
    }
}