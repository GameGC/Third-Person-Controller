using System;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features.Move
{
    [Serializable]
    public class MoveFeature : BaseMoveFeature
    {
        public MoveFeature()
        {
            rotationSpeed = 16f;
            runningSpeed = 4f;
        }


        public override void SetControllerMoveSpeed(float movementSmooth, in float dt)
        {
            Variables.MoveSpeed = Mathf.Lerp( Variables.MoveSpeed, runningSpeed, movementSmooth * dt);
        }
    
        public override void OnFixedUpdateState()
        {
            float dt = Time.fixedDeltaTime;

            // update position
        
            SetControllerMoveSpeed(Variables.MovementSmooth, in dt);
            MoveCharacter(Variables.IsSlopeBadForMove, Input.moveDirection);

            // update rotation
        
            if (Input.moveInputMagnitude >0) 
                RotateToDirection(Input.moveDirection, in dt);
        
            //update animation
            UpdateAnimation(Variables.IsSlopeBadForMove);
        }
    }
}