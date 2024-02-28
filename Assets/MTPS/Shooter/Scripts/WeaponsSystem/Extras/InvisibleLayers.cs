using Cinemachine;
using UnityEngine;

namespace MTPS.Shooter.WeaponsSystem.Extras
{
    public class InvisibleLayers : CinemachineExtension
    {
        public LayerMask newLayerMask;

        private Camera _camera;
        private int _previousLayerMask;

        protected override void OnEnable()
        {
            var brain = CinemachineCore.Instance.FindPotentialTargetBrain(VirtualCamera);
            _camera = brain.OutputCamera;
            _previousLayerMask = _camera.cullingMask;
        }

        private void OnDisable()
        {
            _camera.cullingMask = _previousLayerMask;
        }

        public override bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
        {
            _camera.cullingMask = newLayerMask.value;
            return false;
        }

        public override void PrePipelineMutateCameraStateCallback(CinemachineVirtualCameraBase vcam, ref CameraState curState, float deltaTime)
        {
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
        }
    }
}
