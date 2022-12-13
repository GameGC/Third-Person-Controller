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
       
        [SerializeField] private Vector3 _capsuleCenter;
        [SerializeField] private float _capsuleHeight;
        [SerializeField] private float _capsuleRadius;
    
        [SerializeField] private float _enterTransitionTime = 0.2f;
        [SerializeField] private float _exitTransitionTime = 0.1f;
        
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
        [SerializeField] private int _capsuleDirection;

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

            _capsuleCollider.center = _capsuleCenter;
            _capsuleCollider.height = _capsuleHeight;
            _capsuleCollider.radius = _capsuleRadius;
            _capsuleCollider.direction = _capsuleDirection;

            _animator.CrossFadeInFixedTime("Prone", .2f);
            
            // camera reposition logic
            
            _positionConstraint.enabled = true;
            WaitAndDisableCameraConstraint(_enterTransitionTime);
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
        
            _animator.CrossFadeInFixedTime("DefaultMove", _exitTransitionTime);
            
            // camera reposition logic
            
            _positionConstraint.enabled = true;
            WaitAndDisableCameraConstraint(_exitTransitionTime);
        }
    }
}