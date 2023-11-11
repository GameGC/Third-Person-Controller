using System;
using Cinemachine;
using Fighting.Pushing;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;
using UnityEngine.Playables;
using Weapons;

public class WeaponShootingInfo : MonoBehaviour,IWeaponInfo
{
    public DefaultRaycastBullet prefab;
    public Transform spawnPoint;
    
    public GameObject muzzle;
    public float muzzleTime = 1;

    public bool hasAutoReloadOnStart = true;
    public int totalAmmo = 1000; 
    public int ammoImMagazine = 5;
    
    public float cooldownTime = 3;
    [ClipToSeconds]
    public float reloadingTime = 10;

    public bool calcFormat;
    public int maxShootsPerMinute;
    
    private int remainingAmmo;

    private IFightingStateMachineVariables Variables;
    
    private CinemachineImpulseSource _impulseSource;
    private ShellDispancer _shellDispancer;

    public void CacheReferences(IFightingStateMachineVariables variables)
    {
        Variables = variables;
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _shellDispancer = GetComponent<ShellDispancer>();

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

    public void Fired() => Debug.LogError("event fired");


    private void AutoReload()
    {
        if (totalAmmo - ammoImMagazine <= 0) return;
        totalAmmo -= ammoImMagazine;
        remainingAmmo = ammoImMagazine;
    }

    public void Shoot()
    { 
        if(Variables.isCooldown)   throw new Exception("Wrong code execution, check conditions in executor class");
        if(Variables.isReloading)  throw new Exception("Wrong code execution, check conditions in executor class");
        if(!Variables.couldAttack) throw new Exception("Wrong code execution, check conditions in executor class");
        
        if (remainingAmmo > 0)
        {
            remainingAmmo--;
            Variables.couldAttack = remainingAmmo > 0;


            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            
            _impulseSource?.GenerateImpulse();
            
            _shellDispancer?.OnShoot();
            
            if (muzzle)
            {
                muzzle.SetActive(true);
                Invoke(nameof(DisableMuzzle),muzzleTime);
            }
        }

        //reload
        if (remainingAmmo < 1)
        {
            if (totalAmmo - ammoImMagazine <= 0) return;
            Variables.isReloading = true;
            Invoke(nameof(Reload),reloadingTime);
        }
        //cooldown
        else
        {
            Variables.isCooldown = true;
            Invoke(nameof(Cooldown),cooldownTime);
        }
    }

    private void Reload()
    {
        totalAmmo -= ammoImMagazine;
        remainingAmmo = ammoImMagazine;
        Variables.isReloading = false;
        Variables.couldAttack = true;
    }
    
    private void Cooldown() => Variables.isCooldown = false;

    private void DisableMuzzle() =>muzzle.SetActive(false);


    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(spawnPoint.position,spawnPoint.position+spawnPoint.forward*100);
    }


    private void OnDisable()
    {
        CancelInvoke();
    }
}

public interface IWeaponInfo
{
    public void CacheReferences(IFightingStateMachineVariables variables);

    public void Shoot();
}

public abstract class GunModule : MonoBehaviour
{
    protected IFightingStateMachineVariables Variables;

    public virtual void OnAfterShoot(){}
}