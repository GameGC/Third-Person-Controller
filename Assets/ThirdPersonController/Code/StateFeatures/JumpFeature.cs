using System;
using System.Timers;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features
{
    [Serializable]
    public class JumpFeature : BaseFeature
    {
        // animation hashes
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int JumpDuringMove = Animator.StringToHash("JumpMove");
        
        
        [Tooltip("How much time the character will be jumping")]
        [SerializeField] private float jumpTimer = 0.3f;
        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        [SerializeField] private float jumpHeight = 4f;
    
        private IMoveStateMachineVariables _variables;
        private Animator _animator;
        private Rigidbody _rigidbody;
        private BaseInputReader _input;
    
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;
        
            _animator = resolver.GetComponent<Animator>();
            _rigidbody = resolver.GetComponent<Rigidbody>();
            _input = resolver.GetComponent<BaseInputReader>();
        }

        public override void OnUpdateState()
        {
            if (_input.isJump && CouldJump()) 
                DoJump();
        }
    
        private bool CouldJump()
        {
            return _variables.JumpCounterElapsed && !_variables.IsSlopeBadForMove;
        }

        private void DoJump()
        {
            var timer = new Timer(jumpTimer * 1000) {AutoReset = false};
            timer.Elapsed += (sender, args) =>
            {
                _variables.JumpCounterElapsed = true;
                timer.Dispose();
            };
            timer.Start();

            _variables.JumpCounterElapsed = false;
        
            // trigger jump physics
            _rigidbody.AddForce(Vector3.up * jumpHeight,ForceMode.VelocityChange);
        
            // trigger jump animations
            if (_input.moveInput == Vector2.zero)
                _animator.CrossFadeInFixedTime(Jump, 0.1f);
            else
                _animator.CrossFadeInFixedTime(JumpDuringMove, .2f);
      
        }
    }
}