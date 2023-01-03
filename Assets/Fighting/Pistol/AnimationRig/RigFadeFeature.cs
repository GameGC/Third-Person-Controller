using System;
using UnityEngine;

[Serializable]
public class RigFadeFeature : BaseRigFeature
{
    [SerializeField] private float fadeMax = 1;
    [SerializeField] private float deltaMultiplayer = 1;

    private float tempLerp;

    public override void OnEnterState()
    {
        tempLerp = 0;
        base.OnEnterState();
    }

    public override void OnUpdateState()
    {
        tempLerp += deltaMultiplayer * Time.deltaTime;
        _targetLayer.rig.weight = Mathf.Lerp(0, fadeMax, tempLerp);
    }
}