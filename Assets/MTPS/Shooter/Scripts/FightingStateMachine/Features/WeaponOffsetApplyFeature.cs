using System.Linq;
using MTPS.Core;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon.Extensions;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Features
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
            _animationLayer = _variables.AnimationLayer;
        }

        public override async void OnEnterState()
        {
            base.OnEnterState();
            targetOffset ??= 
                _variables.weaponInstance.GetComponents<WeaponOffset>().FirstOrDefault(o => o.applyWhen == applyWhenName);
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