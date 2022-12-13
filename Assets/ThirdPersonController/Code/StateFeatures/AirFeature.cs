using System;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features
{
    [Serializable]
    public class AirFeature : BaseFeature
    {
        [Tooltip("Speed that the character will move while airborne")]
        [SerializeField] private float airSpeed = 5f;
        [Tooltip("Smoothness of the direction while airborne")]
        [SerializeField] private float airSmooth = 6f;

        [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
        [SerializeField] private bool jumpWithRigidbodyForce = true;
        [Tooltip("Rotate or not while airborne")]
        [SerializeField] private bool jumpAndRotate = true;
    
        [Tooltip("Rotation speed of the character")]
        [SerializeField] private float rotationSpeed = 16f;


        private Transform _transform;
        private Rigidbody _rigidbody;
        private BaseInputReader _input;


        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _transform = resolver.GetComponent<Transform>();
            _rigidbody = resolver.GetComponent<Rigidbody>();
            _input = resolver.GetComponent<BaseInputReader>();
        }

        public override void OnFixedUpdateState()
        {
            float dt = Time.fixedDeltaTime;
        
            if (jumpAndRotate)
                RotateToDirection(_input.moveDirection, in dt);
        
            if (jumpWithRigidbodyForce)
            {
                _rigidbody.AddForce(_input.moveDirection * (airSpeed * dt), ForceMode.VelocityChange);
                return;
            }

            Vector3 targetVelocity = _input.moveDirection * airSpeed;
            targetVelocity.y = _rigidbody.velocity.y;

            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, targetVelocity, airSmooth * dt);
        }

        private void RotateToDirection(Vector3 direction,in float dt)
        {
            Vector3 desiredForward = Vector3.RotateTowards(_transform.forward, direction, rotationSpeed * dt, .1f);
            Quaternion newRotation = Quaternion.LookRotation(desiredForward);
            _transform.rotation = newRotation;
        }
    }
}