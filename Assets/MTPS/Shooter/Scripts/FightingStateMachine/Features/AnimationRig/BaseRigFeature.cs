using MTPS.Core;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MTPS.Shooter.FightingStateMachine.Features.AnimationRig
{
    public abstract class BaseRigFeature : BaseFeatureWithAwaiters
    {
        //[SerializeField] protected string waitForStateWeight;
        [SerializeField] private RigTypes layerType = RigTypes.Fighting;
        [SerializeField] private bool returnPreviousValueOnExit = true;
    
        protected RigLayer _targetLayer;
        private float _previousValue;

        protected AnimationLayer _animationLayer;
    
    
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            //if (!string.IsNullOrEmpty(waitForStateWeight))
            if (variables is IFightingStateMachineVariables fighting)
                _animationLayer = fighting.AnimationLayer;
            else if(variables is ICollectStateMachineVariables collect) 
                _animationLayer = collect.AnimationLayer;

            _targetLayer = resolver.GetComponent<RigBuilder>().layers[(int) layerType];
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            _previousValue = _targetLayer.rig.weight;
        }

        public override void OnExitState()
        {
            base.OnExitState();
            if (returnPreviousValueOnExit)
                _targetLayer.rig.weight = _previousValue;
        }
    }
}