using MTPS.Core;
using MTPS.Shooter.FightingStateMachine;

namespace MTPS.Shooter.WeaponsSystem.ShootableWeapon
{
    public interface IWeaponInfo
    {
        public void CacheReferences(IFightingStateMachineVariables variables,IReferenceResolver resolver);

        public void Shoot();
    
        public int remainingAmmo { get; }
        public int maxAmmo { get; }

        public float reloadingOrCooldownTime { get; }
    }
}