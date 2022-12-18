using System;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features
{
    [Serializable]
    public class CheckSlopeFeature : BaseFeature
    {
        [Tooltip("Max angle to walk")]
        [Range(30, 80),SerializeField] private float slopeLimit = 45f;


        private Transform _transform;
        private CapsuleCollider _capsuleCollider;
        private BaseInputReader _input;
        private IMoveStateMachineVariables _variables;

        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;

            _transform = resolver.GetComponent<Transform>();
            _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
            _input = resolver.GetComponent<BaseInputReader>();
        }

        public override void OnFixedUpdateState() => CheckSlopeLimit();

        private void CheckSlopeLimit()
        {
            float halfHeight = _capsuleCollider.height * 0.5f;
            float radius = _capsuleCollider.radius;
            var moveDirection = _input.moveDirection;
            
            
            Vector3 rayStart = _transform.position + Vector3.up * halfHeight;
            if (Physics.Linecast(rayStart, _transform.position + moveDirection * (radius + 0.2f), out var hitInfo, _variables.GroundLayer,QueryTriggerInteraction.Ignore))
            {
                float hitAngle = Vector3.Angle(Vector3.up, hitInfo.normal);

                var targetPoint = hitInfo.point + moveDirection * radius;

                if ((hitAngle > slopeLimit) && Physics.Linecast(rayStart, targetPoint, out hitInfo, _variables.GroundLayer,QueryTriggerInteraction.Ignore))
                {
                    hitAngle = Vector3.Angle(Vector3.up, hitInfo.normal);
                    _variables.SlopeAngle = hitAngle;

                
                    if (hitAngle > slopeLimit && hitAngle < 85f)
                        _variables.IsSlopeBadForMove = true;
                    else
                        _variables.IsSlopeBadForMove = false;
                }
                else
                {
                    _variables.SlopeAngle = hitAngle;
                    _variables.IsSlopeBadForMove = false;
                }
            }
            else
            {
                _variables.IsSlopeBadForMove = false;
            }
        }
    }
}