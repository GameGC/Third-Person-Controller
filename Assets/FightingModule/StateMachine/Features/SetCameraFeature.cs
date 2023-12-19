using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

public class SetCameraFeature : BaseFeature
{
    [SerializeField] private CameraType _cameraType;

    private CameraManager _cameraManager;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _cameraManager = resolver.GetNamedComponent<CameraManager>("CameraManager");
    }

    public override void OnEnterState()
    {
        _cameraManager.SetActiveCamera(_cameraType);
    }

    public override void OnExitState()
    {
    }
}