using System;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public class AimLookFeature : BaseFeature
{
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float forceLookDegree = 45;
    
    private Transform _targetLookTransform;
    private Transform _bodyTransform;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _bodyTransform = resolver.GetComponent<Transform>();
        _targetLookTransform =resolver.GetNamedComponent<Transform>("TargetLook");
    }
    
    public override void OnUpdateState()
    {
        float currentRotation =  _bodyTransform.eulerAngles.y;
        float targetRotation = _targetLookTransform.eulerAngles.y;

        if (Mathf.Abs(currentRotation - targetRotation) > forceLookDegree)
        {
            _bodyTransform.rotation = 
                Quaternion.AngleAxis(targetRotation,_bodyTransform.up);
        }
        else _bodyTransform.rotation = 
            Quaternion.AngleAxis(Mathf.MoveTowardsAngle(currentRotation,targetRotation,Time.deltaTime*rotationSpeed),_bodyTransform.up);
    }
}

public class SetCameraFeature : BaseFeature
{
    [SerializeField] private CameraManager.CameraType _cameraType;
    
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
    }

    public override void OnEnterState()
    {
        GameObject.FindObjectOfType<CameraManager>().SetActiveCamera(_cameraType);
    }

    public override void OnExitState()
    {
    }
}