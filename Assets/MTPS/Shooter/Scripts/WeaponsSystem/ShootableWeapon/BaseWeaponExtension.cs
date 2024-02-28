using UnityEngine;

namespace MTPS.Shooter.WeaponsSystem.ShootableWeapon
{
    public abstract class BaseWeaponExtension : MonoBehaviour
    {
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Calls after shoot happen
        /// </summary>
        public virtual void OnShoot(){ }
        // ReSharper disable Unity.PerformanceAnalysis
    
        public virtual void OnBeginReload(){ }
        // ReSharper disable Unity.PerformanceAnalysis

        public virtual void OnEndReload(){ }
        // ReSharper disable Unity.PerformanceAnalysis

        public virtual void OnBeginCooldown(){ }
        // ReSharper disable Unity.PerformanceAnalysis

        public virtual void OnEndCooldown(){ }
   
    }
}