using System;
using MTPS.Core;
using MTPS.Movement.Core;
using MTPS.Movement.Core.Input;
using UnityEngine;

namespace MTPS.Movement.Features.AlwaysExecuted
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

    
    
        private IMoveInput _input;


        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;
        
            _rigidbody = resolver.GetComponent<Rigidbody>();
            _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
            _transform = resolver.GetComponent<Transform>();
        
            _animator = resolver.GetComponent<Animator>();

            _input = resolver.GetComponent<IMoveInput>();

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
                name = "slippyPhysics",
                staticFriction = 0f,
                dynamicFriction = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum
            };

            // air physics 
            _maxFrictionPhysics = new PhysicMaterial
            {
                name = "maxFrictionPhysics",
                staticFriction = 1f,
                dynamicFriction = 1f,
                frictionCombine = PhysicMaterialCombine.Maximum
            };
        }


        public override void OnUpdateState()
        {
            _animator.SetBool(IsGrounded, _variables.IsGrounded);
            _animator.SetFloat(GroundDistance, _variables.GroundHit.distance);
        }

        public override void OnFixedUpdateState()
        {
            CalculateGroundDistance(_transform,ref _capsuleCollider);
            ControlMaterialPhysics(_variables.IsGrounded, _input.moveInputMagnitude);
            
            _variables.IsGrounded = _variables.GroundHit.distance < groundMinDistance;
            
            if (_variables.JumpCounterElapsed && _variables.GroundHit.distance> 0.08f)
                    _rigidbody.AddForce(_transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
        }


        private void ControlMaterialPhysics(in bool isGrounded,in float inputMagnitude)
        {
            _capsuleCollider.material = isGrounded ? inputMagnitude > 0 ?  _frictionPhysics : _maxFrictionPhysics : _slippyPhysics;
        }

        private void CalculateGroundDistance(Transform transform,ref CapsuleCollider capsuleCollider)
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

            groundHit.distance = (float) Math.Round(distance, 2);
            _variables.GroundHit = groundHit;
        }
    }

    public class MovementSoundsFeature : BaseFeature
    {
        public AudioClip walk;
        public AudioClip run;
        public AudioClip sprint;
        
        private AudioSource source;

        private IMoveStateMachineVariables _variables;
        private IMoveInput _input;
        
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;
            _input = resolver.GetComponent<IMoveInput>();
        }

        public override void OnUpdateState()
        {
            if(!_variables.IsGrounded) return;
            
            if (_input.moveInputMagnitude < 1.01f)
            {
                if (_input.moveInputMagnitude < 0.501f)
                {
                    if (_input.isProne)
                    {
                        
                    }
                    else
                    {
                        //walk
                    }
                }
                else
                {
                    //run
                }
            }
            else
            {
                //sprint
            }

            if (_input.isJump)
            {
                //jump
            }
        }
    }
}