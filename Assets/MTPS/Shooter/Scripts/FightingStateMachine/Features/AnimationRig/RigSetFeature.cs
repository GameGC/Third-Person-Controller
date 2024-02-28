using System;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Features.AnimationRig
{
    [Serializable]
    public class RigSetFeature : BaseRigFeature
    {
        [SerializeField] private float weight;

        public override async void OnEnterState()
        {
            base.OnEnterState();
            if (_animationLayer && weight > 0)
            {
                await _animationLayer.WaitForNextState();
                //avoid wrong execution
                if(!IsRunning) return;
            }

            _targetLayer.rig.weight = weight;
        }
    }
}