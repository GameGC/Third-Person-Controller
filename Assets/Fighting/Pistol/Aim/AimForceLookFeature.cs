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
        _bodyTransform.rotation = Quaternion.AngleAxis(_targetLookTransform.eulerAngles.y,_bodyTransform.up);
    }
}