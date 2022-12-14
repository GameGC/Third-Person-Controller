using System;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features.Move
{
    [Serializable]
    public class SprintFeature : BaseMoveFeature
    {
        public SprintFeature()
        {
            rotationSpeed = 16f;
            runningSpeed = 6f;
        }
    

        public override void SetControllerMoveSpeed(float movementSmooth, in float dt)
        {
            Variables.MoveSpeed = Mathf.Lerp(Variables.MoveSpeed, runningSpeed, movementSmooth * dt);
        }

        private bool _useNewInputSystem;
    
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            base.CacheReferences(variables,resolver);
            _useNewInputSystem = Input.GetType().Name.Contains("ThirdPersonNewInput");
        }

        public override void OnEnterState()
        {
            if(_useNewInputSystem)
                Input.moveInputMagnitude += 0.5f;
        }

        public override void OnFixedUpdateState()
        {
            float dt = Time.fixedDeltaTime;

            SetControllerMoveSpeed(Variables.MovementSmooth, in dt);
            MoveCharacter(Variables.IsSlopeBadForMove, Input.moveDirection);

            if (Input.moveInput != Vector2.zero) 
                RotateToDirection(Input.moveDirection, in dt);

            if(!_useNewInputSystem)
                Input.moveInputMagnitude += 0.5f;
        
            //update animation
            UpdateAnimation(Variables.IsSlopeBadForMove);
        }

        public override void OnExitState()
        {
            if(_useNewInputSystem)
                Input.moveInputMagnitude = Mathf.Clamp01(Input.moveInputMagnitude);
        }
    }
}