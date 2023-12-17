using System.Linq;
using Fighting.Pushing;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine;

namespace Weapons
{
    public class WeaponOffsetApplyFeature : BaseFeatureWithAwaiters
    {
        public string applyWhenName;
        public bool waitForStateWeight1;

        private IFightingStateMachineVariables _variables;
        private AnimationLayer _animationLayer;
        
        private WeaponOffset targetOffset;
        private Pose _previousPose;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
            _animationLayer = (_variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
        }

        public override async void OnEnterState()
        {
            base.OnEnterState();
            targetOffset ??= 
                _variables.weaponInstance.GetComponents<WeaponOffset>().First(o => o.applyWhen == applyWhenName);
            if (waitForStateWeight1)
            {
                await _animationLayer.WaitForNextState();
                if(!IsRunning) return;
            }

            targetOffset.transform.GetLocalPositionAndRotation(out _previousPose.position,out _previousPose.rotation);
            targetOffset.Apply();
        }

        public override void OnExitState()
        {
            base.OnExitState();
            targetOffset.SetCustomPose(_previousPose);
        }
    }
}