using System;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public class AimLookFeature : BaseFeature
{
    [SerializeField] private float rotationSpeed = 1;
    
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
        
        float currentRotation = bodyEuler.y;
        float targetRotation = _targetLookTransform.eulerAngles.y;
        
        _bodyTransform.eulerAngles = 
            new Vector3(bodyEuler.x,
                Mathf.MoveTowardsAngle(currentRotation,targetRotation,Time.deltaTime*rotationSpeed),
                bodyEuler.z);
    }
}