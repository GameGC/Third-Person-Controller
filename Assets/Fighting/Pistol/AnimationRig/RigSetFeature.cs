using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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