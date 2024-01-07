using System;
using Cinemachine;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine;
using UTPS.Inventory;
using UTPS.Inventory.ItemTypes;

public class ShootingWeapon : BaseWeaponWithExtensions,IWeaponInfo
{
    public Transform spawnPoint;
    
    public bool hasAutoReloadOnStart = true;
    public int ammoImMagazine = 5;
    
    public float cooldownTime = 3;
    [ClipToSeconds]
    public float reloadingTime = 10;

    public bool calcFormat;
    public int maxShootsPerMinute;
    

    private IFightingStateMachineVariables Variables;
    
    private CinemachineImpulseSource _impulseSource;
    private Inventory _inventory;
    private AmmoData _currentAmmoType;
    
    public void CacheReferences(IFightingStateMachineVariables variables,IReferenceResolver resolver)
    {
        _inventory = resolver.GetComponent<Inventory>();
        _currentAmmoType = ((WeaponData) _inventory.EquipedItemData).ammoItem;
        
        Variables = variables;
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        if (calcFormat)
        {
            float reloadsPerMinute = (float)maxShootsPerMinute / ammoImMagazine;
            reloadingTime = 60f / reloadsPerMinute;
        }
    }
    private void Start()
    {
        if (hasAutoReloadOnStart) 
            AutoReload();
    }

    private void AutoReload()
    {
        if (_inventory.MinusItem(_currentAmmoType, ammoImMagazine)) return;
        remainingAmmo = ammoImMagazine;
    }

    public void Shoot()
    { 
        if(Variables.isCooldown)   throw new Exception("Wrong Cooldown execution, check conditions in executor class");
        if(Variables.isReloading)  throw new Exception("Wrong Reloading execution, check conditions in executor class");
        if(!Variables.couldAttack) throw new Exception("Wrong CouldAttack execution, check conditions in executor class");
        
        if (remainingAmmo > 0)
        {
            remainingAmmo--;
            Variables.couldAttack = remainingAmmo > 0;


            Instantiate(_currentAmmoType.bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            
            Execute_OnShootExtensions();

            _impulseSource?.GenerateImpulse();
        }
        

        //reload
        if (remainingAmmo < 1)
        {  
            if (_inventory.AllItems.TryGetValue(_currentAmmoType, out var totalAmmo) && totalAmmo - ammoImMagazine <= 0) return; 
            Variables.isReloading = true;
            
            Execute_BeginReloadExtensions();
            Invoke(nameof(Reload),reloadingTime);
        }
        //cooldown
        else
        {
            Variables.isCooldown = true;
            
            Execute_BeginCooldownExtensions();
            Invoke(nameof(Cooldown),cooldownTime);
        }
    }

    public int remainingAmmo { get; private set; }
    public int maxAmmo => ammoImMagazine;

    private void Reload()
    {
        if (_inventory.MinusItem(_currentAmmoType, ammoImMagazine)) 
            remainingAmmo = ammoImMagazine;
        Variables.isReloading = false;
        Variables.couldAttack = true;
        Execute_EndReloadExtensions();
    }
    
    private void Cooldown()
    {
        Variables.isCooldown = false;
        Execute_EndCooldownExtensions();
    }

    private void OnDrawGizmos() => Gizmos.DrawRay(spawnPoint.position,spawnPoint.forward*100);

    private void OnDisable() => CancelInvoke();
}