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
    }
}