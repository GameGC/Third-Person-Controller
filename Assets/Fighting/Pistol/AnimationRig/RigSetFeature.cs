using System;
using UnityEngine;

[Serializable]
public class RigSetFeature : BaseRigFeature
{
    [SerializeField] private float weight;

    public override void OnEnterState()
    {
        base.OnEnterState();
        _targetLayer.rig.weight = weight;
        //Debug.LogError(_targetLayer);
        //Debug.LogError( _targetLayer.rig);
        //Debug.LogError( _targetLayer.rig.weight);
    }
}