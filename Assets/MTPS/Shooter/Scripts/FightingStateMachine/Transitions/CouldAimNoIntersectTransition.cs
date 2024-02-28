using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Transitions
{
    public class CouldAimNoIntersectTransition : BaseStateTransition
    {
#if UNITY_EDITOR
        [SerializeField] private bool visualiseRaycast;
#endif
    
        private CapsuleCollider _capsuleCollider;
        private Transform _transform;

        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = (variables as IFightingStateMachineVariables);
            _transform = resolver.GetComponent<Transform>();
            _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
        }

        public override bool couldHaveTransition
        {
            get
            {
                if (float.IsPositiveInfinity(_variables.MinAimingDistance)) return true;
                
                var ray = new Ray(_transform.position + Vector3.up * _capsuleCollider.height / 2, _transform.forward);
#if UNITY_EDITOR
                if(visualiseRaycast)
                    Debug.DrawRay(ray.origin, ray.direction * (_capsuleCollider.radius + _variables.MinAimingDistance),new Color(0.34f, 0.64f, 0.8f));
#endif
                return !Physics.Raycast(ray, _capsuleCollider.radius + _variables.MinAimingDistance, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);
            }
        }
    }
}