using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.DI;

public interface IWeaponInfo
{
    public void CacheReferences(IFightingStateMachineVariables variables,IReferenceResolver resolver);

    public void Shoot();
    
    public int remainingAmmo { get; }
    public int maxAmmo { get; }

    public float reloadingOrCooldownTime { get; }
}