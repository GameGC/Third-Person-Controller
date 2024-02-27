using MTPS.Core;
using ThirdPersonController.Code.AnimatedStateMachine;

public interface IWeaponInfo
{
    public void CacheReferences(IFightingStateMachineVariables variables,IReferenceResolver resolver);

    public void Shoot();
    
    public int remainingAmmo { get; }
    public int maxAmmo { get; }

    public float reloadingOrCooldownTime { get; }
}