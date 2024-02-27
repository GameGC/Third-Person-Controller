using System;
using System.Timers;
using MTPS.Core;
using MTPS.Movement.Core;
using MTPS.Movement.Core.Input;
using UnityEngine;

namespace MTPS.Movement.Features
{
    [Serializable]
    public class JumpFeature : BaseFeature
    {
        // animation hashes
        private static readonly int Jump = Animator.StringToHash("JumpIdle");
        private static readonly int JumpDuringMove = Animator.StringToHash("JumpMove");
        
        
        [Tooltip("How much time the character will be jumping")]
        [SerializeField] private float jumpTimer = 0.3f;
        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        [SerializeField] private float jumpHeight = 4f;
    
        private IMoveStateMachineVariables _variables;
        private Animator _animator;
        private Rigidbody _rigidbody;
        private IMoveInput _input;
    
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;
        
            _animator = resolver.GetComponent<Animator>();
            _rigidbody = resolver.GetComponent<Rigidbody>();
            _input = resolver.GetComponent<IMoveInput>();
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
            if (_input.moveInputMagnitude < 0.01f)
                _animator.CrossFadeInFixedTime(Jump, 0.1f);
            else
                _animator.CrossFadeInFixedTime(JumpDuringMove, .2f);
      
        }
    }
}