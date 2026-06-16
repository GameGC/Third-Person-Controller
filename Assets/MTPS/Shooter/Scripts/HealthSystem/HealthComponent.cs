using System;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class HealthComponent : MonoBehaviour , IHealthVariable
{
    [field: SerializeField] public float Health { get; protected set; } = 100;

    public event Action<float, float> OnHealthChanged;

    public void SetHealth(float value)
    {
        float prev = Health;
        Health = value;
        if (!Mathf.Approximately(prev, value))
            OnHealthChanged?.Invoke(prev, value);
    }

    public virtual void OnHit(RaycastHit hit,IDamageSender source)
    {
        SetHealth(source.damage);
    }
}