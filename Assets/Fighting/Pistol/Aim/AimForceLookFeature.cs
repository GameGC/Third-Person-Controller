using System;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public class AimForceLookFeature : BaseFeature
{
    private Transform _targetLookTransform;
    private Transform _bodyTransform;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _bodyTransform = resolver.GetComponent<Transform>();
        _targetLookTransform =resolver.GetNamedComponent<Transform>("TargetLook");
    }

    public override void OnUpdateState()
    {
        var bodyEuler = _bodyTransform.eulerAngles;
        _bodyTransform.eulerAngles = new Vector3(bodyEuler.x,_targetLookTransform.eulerAngles.y,bodyEuler.z);
    }
}