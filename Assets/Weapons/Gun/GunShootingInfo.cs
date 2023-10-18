using System;
using Cinemachine;
using Fighting.Pushing;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;

public class GunShootingInfo : MonoBehaviour
{
    public Transform prefab;
    public Transform spawnPoint;
    
    public GameObject muzzle;
    public float muzzleTime = 1;

    public bool hasAutoReloadOnStart = true;
    public int totalAmmo = 1000; 
    public int ammoImMagazine = 5;
    
    
    
    
    public float cooldownTime = 3;
    public float reloadingTime = 10;



    private int remainingAmmo;

    private IFightingStateMachineVariables Variables;
    private CinemachineImpulseSource _impulseSource;

    private void Start()
    {
        Variables = FindObjectOfType<FightingStateMachineVariables>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (hasAutoReloadOnStart) 
            AutoReload();
    }


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


    void OnDrawGizmos()
    {
        Gizmos.DrawLine(spawnPoint.position,spawnPoint.position+spawnPoint.forward*100);
    }


    private void OnDisable()
    {
        CancelInvoke();
    }
}
