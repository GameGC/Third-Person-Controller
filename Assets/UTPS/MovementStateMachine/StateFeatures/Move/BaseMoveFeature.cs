using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features.Move
{
    public abstract class BaseMoveFeature : BaseFeature
    {
        // animation hashes
        private static readonly int InputMagnitude = Animator.StringToHash("InputMagnitude");

        [Range(0f, 1f)]
        [SerializeField] private float animationSmooth = 0.2f;

        [Tooltip("Rotation speed of the character")]
        [SerializeField] protected float rotationSpeed = 16f;
        [Tooltip("Speed of character move using rigidbody")]
        [SerializeField] protected float runningSpeed = 4f;
    
        protected IBaseInputReader Input;
        protected IMoveStateMachineVariables Variables;
        
        private Animator _animator;
        private Rigidbody _rigidbody;
        private Transform _transform;


        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            Variables = variables as IMoveStateMachineVariables;
        
            _rigidbody = resolver.GetComponent<Rigidbody>();
            _animator = resolver.GetComponent<Animator>();
            _transform = resolver.GetComponent<Transform>();
            
            Input = resolver.GetComponent<IBaseInputReader>();
        }


        public abstract void SetControllerMoveSpeed(float movementSmooth, in float dt);


        // we don't use delta time because already used in smoothing of input and move speed... and velocity is constant
        protected void MoveCharacter(bool stopMove,Vector3 direction)
        {
            Vector3 targetVelocity = direction * (stopMove || Input.isInputFrozen ? 0 : Variables.MoveSpeed);
            targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = targetVelocity;
        }

        protected void RotateToDirection(Vector3 direction,in float dt)
        {
            Vector3 desiredForward = Vector3.RotateTowards(_transform.forward, direction, rotationSpeed * dt, .1f);
            Quaternion newRotation = Quaternion.LookRotation(desiredForward);
            _transform.rotation = newRotation;
        }

        protected void UpdateAnimation(bool stopMove)
        {
            _animator.SetFloat(InputMagnitude, stopMove ? 0f : Input.moveInputMagnitude, animationSmooth, Time.deltaTime);
        }
        
        protected void UpdateAnimation(bool stopMove,float moveInput)
        {
            _animator.SetFloat(InputMagnitude, stopMove ? 0f : moveInput, animationSmooth, Time.deltaTime);
        }
    }
}