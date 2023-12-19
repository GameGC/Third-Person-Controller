using System;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public abstract class BaseHealthFeature
{
    public abstract void CacheReferences(IHealthVariable variables,IReferenceResolver resolver);

    public virtual void OnHit(in float previousHealth,in float newHealth, in RaycastHit hit,IDamageSender damageSender) { }

    public Action<bool> SetActive;
    public Action Destroy;
    public Action<float> DestroyDelayed;
    
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