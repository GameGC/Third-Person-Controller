using System;
using System.Linq;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[Serializable]
public class BaseRigFeature : BaseFeature
{
    [SerializeField] private string layerName;
    [SerializeField] private bool returnPreviousValueOnExit = true;
    
    protected RigLayer _targetLayer;
    private float _previousValue;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _targetLayer = resolver.GetComponent<RigBuilder>().layers.
            FirstOrDefault(l => l.name == layerName);
    }

    public override void OnEnterState()
    {
        _previousValue = _targetLayer.rig.weight;
    }

    public override void OnExitState()
    {
        if (returnPreviousValueOnExit)
            _targetLayer.rig.weight = _previousValue;
    }
}