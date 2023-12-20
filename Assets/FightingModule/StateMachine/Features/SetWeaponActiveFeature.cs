using System.Collections;
using System.Collections.Generic;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

public class SetWeaponActiveFeature : BaseFeature
{
    public bool active;
    
    private IFightingStateMachineVariables _variables;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _variables = variables as IFightingStateMachineVariables;
    }

    public override void OnEnterState()
    {
        _variables.weaponInstance.SetActive(active);
        
        if(_variables.secondaryWeaponInstance)
            _variables.secondaryWeaponInstance.SetActive(active);
    }
}
