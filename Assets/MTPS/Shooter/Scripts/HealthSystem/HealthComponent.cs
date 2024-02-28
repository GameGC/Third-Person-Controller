using GameGC.SurfaceSystem;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class HealthComponent : MonoBehaviour , IHealthVariable
{
    [field: SerializeField] public float Health { get; protected set; } = 100;
    [FormerlySerializedAs("hitEffect")][SerializeField] protected SurfaceEffect defaultHitEffect;

    public virtual void OnHit(RaycastHit hit,IDamageSender source)
    {
        Health = source.damage;
        SurfaceSystem.instance.OnSurfaceHit(hit, (int) source.HitType, defaultHitEffect);
    }
}