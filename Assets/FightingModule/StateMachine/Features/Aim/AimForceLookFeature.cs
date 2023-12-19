using System;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;

[Serializable]
public class AimForceLookFeature : BaseFeature
{
    private Transform _targetLookTransform;
    private Transform _bodyTransform;
    private IBaseInputReader _reader;

    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _bodyTransform = resolver.GetComponent<Transform>();
        _reader = resolver.GetComponent<IBaseInputReader>();
        _targetLookTransform =resolver.GetNamedComponent<Transform>("TargetLook");
    }


    public override void OnFixedUpdateState()
    {
        if (_reader.moveInput.y < 0 || _reader.moveInput.x !=0)
        {
            var forward = Camera.main.transform.forward;
            forward.y = 0;
            _bodyTransform.rotation = Quaternion.LookRotation(forward,_bodyTransform.up);
            return;
        }
        
        _bodyTransform.rotation.ToEuler();
        _bodyTransform.rotation = Quaternion.AngleAxis(_targetLookTransform.eulerAngles.y,_bodyTransform.up);
    }
}