using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class RigFadeFeature : BaseRigFeature
{
    [SerializeField] private float fadeMax = 1;
    [SerializeField] private float deltaMultiplayer = 1;

    private float tempLerp;

    private Task _tempTask;
    public override async void OnEnterState()
    {
        base.OnEnterState();
        if (_animationLayer && fadeMax > 0)
        {
            _tempTask = _animationLayer.WaitForNextState();
            await _tempTask;
            if(!IsRunning) return;
        }

        tempLerp = 0;
    }

    public override void OnUpdateState()
    {
        if(_tempTask!=null && !_tempTask.IsCompleted)return;
        tempLerp += deltaMultiplayer * Time.deltaTime;
        _targetLayer.rig.weight = Mathf.Lerp(0, fadeMax, tempLerp);
    }

    public override void OnExitState()
    {
        base.OnExitState();
        _tempTask = null;
    }
}