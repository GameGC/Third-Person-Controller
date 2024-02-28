using UnityEngine;
using UnityEngine.Serialization;

namespace MTPS.Shooter.WeaponsSystem.ShootableWeapon
{
    public abstract class BaseWeaponWithExtensions : MonoBehaviour
    {
        [FormerlySerializedAs("EDITOR_extensions")] [SerializeField, HideInInspector] internal BaseWeaponExtension[] extensions;
    
        protected virtual void OnValidate()
        {
            extensions = GetComponents<BaseWeaponExtension>();
        }

        protected void Execute_OnShootExtensions()
        {
            foreach (var baseWeaponExtension in extensions)
            {
                baseWeaponExtension.OnShoot();
            }
        }
    
        protected void Execute_BeginReloadExtensions()
        {
            foreach (var baseWeaponExtension in extensions)
            {
                baseWeaponExtension.OnBeginReload();
            }
        }
    
        protected void Execute_EndReloadExtensions()
        {
            foreach (var baseWeaponExtension in extensions)
            {
                baseWeaponExtension.OnEndReload();
            }
        }
    
        protected void Execute_BeginCooldownExtensions()
        {
            foreach (var baseWeaponExtension in extensions)
            {
                baseWeaponExtension.OnBeginCooldown();
            }
        }

        protected void Execute_EndCooldownExtensions()
        {
            foreach (var baseWeaponExtension in extensions)
            {
                baseWeaponExtension.OnEndCooldown();
            }
        }
    }
}