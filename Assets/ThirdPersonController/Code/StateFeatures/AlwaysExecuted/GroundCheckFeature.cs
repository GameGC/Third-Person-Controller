using System;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features
{
    [Serializable]
    public class GroundCheckFeature : BaseFeature
    {
        // animation hashes
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int GroundDistance = Animator.StringToHash("GroundDistance");
    
    
        private static PhysicMaterial _frictionPhysics;
        private static PhysicMaterial _slippyPhysics;
        private static PhysicMaterial _maxFrictionPhysics;
    
        
        [Tooltip("Distance to became not grounded")]
        [SerializeField] private float groundMinDistance = 0.25f;
        [SerializeField] private float groundMaxDistance = 0.5f;
        [Tooltip("Apply extra gravity when the character is not grounded")]
        [SerializeField] private float extraGravity = -10f;
    



        private IMoveStateMachineVariables _variables;
        private CapsuleCollider _capsuleCollider;
        private Rigidbody _rigidbody;
        private Transform _transform;
        private Animator _animator;

    
    
        private BaseInputReader _input;


        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;
        
            _rigidbody = resolver.GetComponent<Rigidbody>();
            _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
            _transform = resolver.GetComponent<Transform>();
        
            _animator = resolver.GetComponent<Animator>();

            _input = resolver.GetComponent<BaseInputReader>();

            //initialise material if needed
            if (_frictionPhysics) return;
        
            // slides the character through walls and edges
            _frictionPhysics = new PhysicMaterial
            {
                name = "frictionPhysics",
                staticFriction = .25f,
                dynamicFriction = .25f,
                frictionCombine = PhysicMaterialCombine.Multiply
            };

            // prevents the collider from slipping on ramps
            _slippyPhysics = new PhysicMaterial
            {
                name = "maxFrictionPhysics",
                staticFriction = 1f,
                dynamicFriction = 1f,
                frictionCombine = PhysicMaterialCombine.Maximum
            };

            // air physics 
            _maxFrictionPhysics = new PhysicMaterial
            {
                name = "slippyPhysics",
                staticFriction = 0f,
                dynamicFriction = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum
            };
        }


        public override void OnUpdateState()
        {
            _animator.SetBool(IsGrounded, _variables.IsGrounded);
            _animator.SetFloat(GroundDistance, _variables.GroundDistance);
        }

        public override void OnFixedUpdateState()
        {
            _variables.GroundDistance = CalculateGroundDistance(_transform,ref _capsuleCollider);
            ControlMaterialPhysics(_variables.IsGrounded,in _input.moveInput);
            
            _variables.IsGrounded = _variables.GroundDistance < groundMinDistance;
       
        
            if (_variables.JumpCounterElapsed && _variables.GroundDistance > 0.08f)
                _rigidbody.AddForce(_transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
        }


        private void ControlMaterialPhysics(in bool isGrounded,in Vector2 input)
        {
            _capsuleCollider.material = isGrounded ? input == Vector2.zero? _maxFrictionPhysics : _frictionPhysics  : _slippyPhysics;
        }

        private float CalculateGroundDistance(Transform transform,ref CapsuleCollider capsuleCollider)
        {
            // radius of the SphereCast
            float radius = capsuleCollider.radius;
                
            float distance = radius + groundMaxDistance;
              
            Vector3 pos = transform.position + Vector3.up * radius;
            Ray ray = new Ray(pos, Vector3.down);
                
            if (Physics.SphereCast(ray, radius * 0.9f, out var groundHit, distance, _variables.GroundLayer,QueryTriggerInteraction.Ignore))
            {
                float newDist = groundHit.distance;
                distance = newDist;
            }
                
            return (float)System.Math.Round(distance, 2);
        }
    }
}