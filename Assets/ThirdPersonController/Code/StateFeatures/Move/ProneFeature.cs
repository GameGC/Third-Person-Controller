using System;
using System.Threading.Tasks;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using UnityEngine;
using UnityEngine.Animations;

namespace ThirdPersonController.MovementStateMachine.Features.Move
{
    [Serializable]
    public class ProneFeature : BaseFeature
    {
       
        [SerializeField] private Vector3 capsuleCenter;
        [SerializeField] private float capsuleHeight;
        [SerializeField] private float capsuleRadius;
    
        [SerializeField] private float enterTransitionTime = 0.2f;
        [SerializeField] private float exitTransitionTime = 0.1f;
        
        /*
     case 0:
          this.m_BoundsHandle.heightAxis = CapsuleBoundsHandle.HeightAxis.X;
          break;
        case 1:
          this.m_BoundsHandle.heightAxis = CapsuleBoundsHandle.HeightAxis.Y;
          break;
        case 2:
          this.m_BoundsHandle.heightAxis = CapsuleBoundsHandle.HeightAxis.Z;
          break;
     */
        [SerializeField] private int capsuleDirection;

        private Vector3 _tempCapsuleCenter;
        private float _tempCapsuleHeight;
        private float _tempCapsuleRadius;
        private int _tempCapsuleDirection;
    
        private CapsuleCollider _capsuleCollider;
        private Animator _animator;
        private PositionConstraint _positionConstraint;

        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _positionConstraint = resolver.GetComponent<PositionConstraint>();
            _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
            _animator = resolver.GetComponent<Animator>();
        }

        public override void OnEnterState()
        {
            _tempCapsuleCenter = _capsuleCollider.center;
            _tempCapsuleHeight = _capsuleCollider.height;
            _tempCapsuleRadius = _capsuleCollider.radius;
            _tempCapsuleDirection = _capsuleCollider.direction;

            _capsuleCollider.center = capsuleCenter;
            _capsuleCollider.height = capsuleHeight;
            _capsuleCollider.radius = capsuleRadius;
            _capsuleCollider.direction = capsuleDirection;

            _animator.CrossFadeInFixedTime("Prone", .2f);
            
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
            _capsuleCollider.radius = _tempCapsuleRadius;
            _capsuleCollider.direction = _tempCapsuleDirection;
        
            _animator.CrossFadeInFixedTime("DefaultMove", exitTransitionTime);
            
            // camera reposition logic
            
            _positionConstraint.enabled = true;
            WaitAndDisableCameraConstraint(exitTransitionTime);
        }
    }
}