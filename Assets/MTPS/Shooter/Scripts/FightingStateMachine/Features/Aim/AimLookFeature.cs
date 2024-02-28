using System;
using MTPS.Core;
using MTPS.Movement.Core.Input;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Features.Aim
{
    [Serializable]
    public class AimLookFeature : BaseFeature
    {
        [SerializeField] private float rotationSpeed = 1;
        [SerializeField] private float forceLookDegree = 45;
    
        private Transform _targetLookTransform;
        private Transform _bodyTransform;

        private IMoveInput _reader;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _bodyTransform = resolver.GetComponent<Transform>();
            _reader = resolver.GetComponent<IMoveInput>();
            _targetLookTransform =resolver.GetNamedComponent<Transform>("TargetLook");
        }
    
        public override void OnFixedUpdateState()
        {
            if (_reader.moveInput.y < 0 || _reader.moveInput.x !=0)
            {
                var forward = Camera.main.transform.forward;
                forward.y = 0;
                _bodyTransform.rotation = Quaternion.LookRotation(forward,_bodyTransform.up);
                return;
            }
        
            float currentRotation =  _bodyTransform.eulerAngles.y;
            float targetRotation = _targetLookTransform.eulerAngles.y;

            if (Mathf.Abs(currentRotation - targetRotation) > forceLookDegree)
            {
                _bodyTransform.rotation = 
                    Quaternion.AngleAxis(targetRotation,_bodyTransform.up);
            }
            else _bodyTransform.rotation = 
                Quaternion.AngleAxis(Mathf.MoveTowardsAngle(currentRotation,targetRotation,Time.deltaTime*rotationSpeed),_bodyTransform.up);
        }
    }
}