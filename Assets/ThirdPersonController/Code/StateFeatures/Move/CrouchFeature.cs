using System;
using System.Threading.Tasks;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using UnityEngine;
using UnityEngine.Animations;

namespace ThirdPersonController.MovementStateMachine.Features.Move
{
    [Serializable]
    public class CrouchFeature : BaseFeature
    {
        [SerializeField] private Vector3 capsuleCenter;
        [SerializeField] private float capsuleHeight;
       
        [SerializeField] private float enterTransitionTime = 0.2f;
        [SerializeField] private float exitTransitionTime = 0.1f;
        
        private Vector3 _tempCapsuleCenter;
        private float _tempCapsuleHeight;

        private CapsuleCollider _capsuleCollider;
        private Animator _animator;
        private PositionConstraint _positionConstraint;


        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
            _animator = resolver.GetComponent<Animator>();
            _positionConstraint = resolver.GetComponent<PositionConstraint>();
        }

        public override void OnEnterState()
        {
            _tempCapsuleCenter = _capsuleCollider.center;
            _tempCapsuleHeight = _capsuleCollider.height;

            _capsuleCollider.center = capsuleCenter;
            _capsuleCollider.height = capsuleHeight;
        
            _animator.CrossFadeInFixedTime("Crouch", enterTransitionTime);
            
            
            // camera reposition logic

            _positionConstraint.enabled = true;
            WaitAndDisableCameraConstraint(enterTransitionTime);
        }

        private async void WaitAndDisableCameraConstraint(float timer)
        {
            await Task.Delay((int) (timer * 1000));
            _positionConstraint.enabled = false;
        }

        public override void OnExitState()
        {
            _capsuleCollider.center = _tempCapsuleCenter;
            _capsuleCollider.height = _tempCapsuleHeight;
        
            _animator.CrossFadeInFixedTime("DefaultMove", exitTransitionTime);
            
                 
            // camera reposition logic

            _positionConstraint.enabled = true;
            WaitAndDisableCameraConstraint(exitTransitionTime);
        }
    }
}