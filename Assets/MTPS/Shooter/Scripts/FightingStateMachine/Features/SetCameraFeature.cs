using MTPS.Core;
using MTPS.Shooter.Cameras;
using UnityEngine;
using CameraType = MTPS.Shooter.Cameras.CameraType;

namespace MTPS.Shooter.FightingStateMachine.Features
{
    public class SetCameraFeature : BaseFeature
    {
        [SerializeField] private CameraType _cameraType;

        private Shooter.Cameras.CameraManager _cameraManager;
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
}