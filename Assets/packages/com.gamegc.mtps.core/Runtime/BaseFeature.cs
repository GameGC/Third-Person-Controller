using System;
using UnityEngine;

namespace MTPS.Core
{
    [Serializable]
    public abstract class BaseFeature
    {
        public abstract void CacheReferences(IStateMachineVariables variables,IReferenceResolver resolver);
    
        public virtual void OnEnterState(){}
        public virtual void OnUpdateState(){}
        public virtual void OnFixedUpdateState(){}
        public virtual void OnExitState(){}

#if UNITY_EDITOR
        // there issues in unity editor ,so this is a fix
        [HideInInspector]
        public string path;

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }
#endif
    }
}