using System;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;

public class WeaponGrenadeInfo : MonoBehaviour,IWeaponInfo
{
    public GrenadeBullet prefab;
    
    public bool hasAutoReloadOnStart = true;
    public int totalAmmo = 1000; 
    
    [ClipToSeconds]
    public float cooldownTime = 3;
    
    
    private IFightingStateMachineVariables Variables;

    public void CacheReferences(IFightingStateMachineVariables variables)
    {
        Variables = variables;
        remainingAmmo = totalAmmo;
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
            var line = transform.root.Find("StateMachines").GetComponentInChildren<BallisticTrajectoryGeneratorPreview>();
            line.GenerateTrajectoryOut(out var points);
            transform.GetChild(0).GetComponent<GrenadeBullet>().enabled = true;
            transform.GetChild(0).GetComponent<GrenadeBullet>().Init(points);
            transform.GetChild(0).SetParent(null);
            
            Variables.isCooldown = true;
            Variables.couldAttack = false;
            Invoke(nameof(Cooldown),cooldownTime);
        }
    }

    public int remainingAmmo { get; private set; }
    public int maxAmmo => totalAmmo;

    private void Cooldown()
    {
        if (remainingAmmo < 1)
        {
            Variables.isCooldown = false;
            Variables.couldAttack = false;
            return;
        }
        
        remainingAmmo--;
        Instantiate(prefab,transform.position,transform.rotation, transform).enabled = false;
        Variables.isCooldown = false;
        Variables.couldAttack = true;
    }
}