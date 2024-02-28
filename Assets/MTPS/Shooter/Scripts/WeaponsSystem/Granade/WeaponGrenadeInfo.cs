using System;
using GameGC.CommonEditorUtils.Attributes;
using MTPS.Core;
using MTPS.Inventory.ItemTypes;
using MTPS.Shooter.FightingStateMachine;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;

namespace MTPS.Shooter.WeaponsSystem.Granade
{
    [DisallowMultipleComponent]
    public class WeaponGrenadeInfo : BaseWeaponWithExtensions,IWeaponInfo
    {
        public bool hasAutoReloadOnStart = true;
    
        [ClipToSeconds]
        public float cooldownTime = 3;

        private GrenadeBullet _tempGranade;
    
        private IFightingStateMachineVariables Variables;
        private Inventory.Inventory _inventory;
        private AmmoData _currentAmmoType;
        public void CacheReferences(IFightingStateMachineVariables variables, IReferenceResolver resolver)
        {
            Variables = variables;
        
            _inventory = resolver.GetComponent<Inventory.Inventory>();
            _currentAmmoType = ((WeaponData) _inventory.EquippedItemData).ammoItem;
        
            if (_inventory.AllItems.TryGetValue(_currentAmmoType, out var value)) 
                remainingAmmo = value;
        }
    
        private void Start()
        {
            if (hasAutoReloadOnStart) 
                Cooldown();
        }

        public void Shoot()
        { 
            if(Variables.isCooldown)   throw new Exception("Wrong code execution, check conditions in executor class");
            if(Variables.isReloading)  throw new Exception("Wrong code execution, check conditions in executor class");
            if(!Variables.couldAttack) throw new Exception("Wrong code execution, check conditions in executor class");

            if (remainingAmmo > 0)
            {
                var line = transform.root.Find("StateMachines").GetComponentInChildren<BallisticTrajectoryGenerator>();
                line.GenerateTrajectoryOut(out var points);
            
                _tempGranade.enabled = true;
                _tempGranade.Init(points);
                _tempGranade.transform.SetParent(null);
            
                Execute_OnShootExtensions();
            
                Variables.isCooldown = true;
                Variables.couldAttack = false;
                Invoke(nameof(Cooldown),cooldownTime);
                Execute_BeginCooldownExtensions();
            }
        }

        public int remainingAmmo { get; private set; }
        public int maxAmmo => _inventory.AllItems[_currentAmmoType];
        public float reloadingOrCooldownTime => cooldownTime;
        private void Cooldown()
        {
            if (remainingAmmo < 1)
            {
                Variables.isCooldown = false;
                Variables.couldAttack = false;
                Execute_EndCooldownExtensions();
                return;
            }
        
            remainingAmmo--;
        
            _tempGranade = Instantiate(_currentAmmoType.bulletPrefab, transform.position, transform.rotation, transform).GetComponent<GrenadeBullet>();
            _tempGranade.enabled = false;
        
            Variables.isCooldown = false;
            Variables.couldAttack = true;
            Execute_EndCooldownExtensions();
        }
    }
}