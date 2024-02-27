using System;
using System.Threading.Tasks;
using MTPS.Core;
using UnityEngine;

namespace MTPS.Movement.Features.Move
{
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

        private async Task MoveToPoint(Pose point)
        {
            var position = point.position;
        
            var targetPos2D = new Vector2(position.x, position.z);
            const float stopDistance = 0.01f;
        
            while ((targetPos2D-new Vector2(_transform.position.x,_transform.position.z)).sqrMagnitude > stopDistance)
            {
                // Debug.Log((targetPos2D-new Vector2(_transform.position.x,_transform.position.z)).sqrMagnitude);
                float dt = Time.deltaTime;

                var direction = (position - _transform.position).normalized;
                direction.y = 0;
            
                // update position
        
                SetControllerMoveSpeed(Variables.MovementSmooth, in dt);

                _transform.position = Vector3.MoveTowards(_transform.position, position, Variables.MoveSpeed * 0.5f * dt);
                _transform.rotation = Quaternion.RotateTowards(_transform.rotation,point.rotation,rotationSpeed * dt);
                // RotateToDirection(direction, in dt);
        
                //update animation
                UpdateAnimation(false,direction.magnitude);
                await WaitFrame();
            }

            _transform.SetPositionAndRotation(point.position,point.rotation);
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
}
