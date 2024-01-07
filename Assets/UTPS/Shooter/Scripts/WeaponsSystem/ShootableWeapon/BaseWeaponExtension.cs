using UnityEngine;

public abstract class BaseWeaponExtension : MonoBehaviour
{
    public virtual void OnShoot(){ }
    
    public virtual void OnBeginReload(){ }
    public virtual void OnEndReload(){ }

    public virtual void OnBeginCooldown(){ }
    public virtual void OnEndCooldown(){ }
}