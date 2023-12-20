using System.Linq;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class InstantiateWeaponAtPivot : BaseFeature
{
    [SerializeField] private string pivotName;
    [SerializeField] private string layerName;
    [SerializeField] private GameObject prefab;

    private RigLayer _targetLayer;
    private IFightingStateMachineVariables _variables;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _targetLayer = resolver.GetComponent<RigBuilder>().layers.
            FirstOrDefault(l => l.name == layerName);
        _variables = variables as IFightingStateMachineVariables;
    }

    public override void OnEnterState()
    {
        var instance = Object.Instantiate(prefab,_targetLayer.rig.transform.Find(pivotName),false);
        _variables.weaponInstance = instance;
        
        if (instance.TryGetComponent<IWeaponInfo>(out var info)) 
            info.CacheReferences(_variables);
    }
}