using System;
using MTPS.Core;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;

[Serializable]
public abstract class BaseHealthFeature
{
    public abstract void CacheReferences(IHealthVariable variables,IReferenceResolver resolver);

    public virtual void OnHit(in float previousHealth,in float newHealth, in Vector3 hitPoint, in Vector3 hitDirection,IDamageSender damageSender) { }

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