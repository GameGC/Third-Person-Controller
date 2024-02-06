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

#if UNITY_EDITOR
        [SerializeField] private bool visualiseRaycast;
#endif

        private Transform _transform;
        private CapsuleCollider _capsuleCollider;
        private IBaseInputReader _input;
        private IMoveStateMachineVariables _variables;

        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;

            _transform = resolver.GetComponent<Transform>();
            _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
            _input = resolver.GetComponent<IBaseInputReader>();
        }

        public override void OnFixedUpdateState() => CheckSlopeLimit();

        private void CheckSlopeLimit()
        {
            float halfHeight = _capsuleCollider.height * 0.5f;
            float radius = _capsuleCollider.radius;
            var moveDirection = _input.moveDirection;
            
//            var ray = new Ray(_transform.position + Vector3.up * _capsuleCollider.height / 2, _transform.forward);
//#if UNITY_EDITOR
//                Debug.DrawRay(ray.origin, ray.direction * (_capsuleCollider.radius + 1.5f),new Color(0.34f, 0.64f, 0.8f));
//#endif
            Vector3 rayStart = _transform.position + Vector3.up * halfHeight;

#if UNITY_EDITOR
            if(visualiseRaycast)
                Debug.DrawLine(rayStart, _transform.position + _input.moveDirection * (radius + 0.2f),Color.yellow);
#endif
            if (Physics.Linecast(rayStart, _transform.position + _input.moveDirection * (radius + 0.2f), out var hitInfo, _variables.GroundLayer,QueryTriggerInteraction.Ignore))
            {
                float hitAngle = Vector3.Angle(Vector3.up, hitInfo.normal);

                var targetPoint = hitInfo.point + moveDirection * radius;

#if UNITY_EDITOR
                if(visualiseRaycast)
                    Debug.DrawLine(rayStart, targetPoint,new Color(0.8f, 0.35f, 0f));
#endif
                if (hitAngle > slopeLimit && Physics.Linecast(rayStart, targetPoint, out hitInfo, _variables.GroundLayer,QueryTriggerInteraction.Ignore))
                {
                    hitAngle = Vector3.Angle(Vector3.up, hitInfo.normal);
                    
                    _variables.SlopeAngle = hitAngle;
                    _variables.IsSlopeBadForMove = hitAngle > slopeLimit;
                }
                else
                {
                    _variables.SlopeAngle = hitAngle;
                    _variables.IsSlopeBadForMove = hitAngle > slopeLimit;
                }
            }
            else
            {
                _variables.SlopeAngle = 0;
                _variables.IsSlopeBadForMove = false;
            }
            
        }
    }
}