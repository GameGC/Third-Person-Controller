using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ThirdPersonController.Core.DI;
using ThirdPersonController.MovementStateMachine.Features.Move;
using UnityEngine;

public class SimpleMoveToPoint : BaseMoveFeature
{   
    [NonSerialized] private Rigidbody _rigidbody;
    [NonSerialized] private Transform _transform;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        base.CacheReferences(variables, resolver);
        base.Input.MoveToPoint += MoveToPoint;
        
        _rigidbody = resolver.GetComponent<Rigidbody>();
        _transform = resolver.GetComponent<Transform>();
    }

    public override void SetControllerMoveSpeed(float movementSmooth, in float dt)
    {
        Variables.MoveSpeed = Mathf.Lerp( Variables.MoveSpeed, runningSpeed, movementSmooth * dt);
    }

    private async Task MoveToPoint(Vector3 pos)
    {
        var targetPos2D = new Vector2(pos.x, pos.z);
        const float stopDistance = 0.01f;
        
        while ((targetPos2D-new Vector2(_transform.position.x,_transform.position.z)).sqrMagnitude > stopDistance)
        {
           // Debug.Log((targetPos2D-new Vector2(_transform.position.x,_transform.position.z)).sqrMagnitude);
            float dt = Time.deltaTime;

            var direction = (pos - _transform.position).normalized;
            direction.y = 0;
            
            // update position
        
            SetControllerMoveSpeed(Variables.MovementSmooth, in dt);

            _transform.position = Vector3.MoveTowards(_transform.position, pos, Variables.MoveSpeed * 0.5f * dt);
           
          //  RotateToDirection(direction, in dt);
        
            //update animation
            UpdateAnimation(false,direction.magnitude);
            await WaitFrame();
        }
    }
    
    
    private async Task WaitFrame()
    {
        var current = Time.frameCount;
 
        while (current == Time.frameCount)
        {
            await Task.Yield();
        }
    }
}
