using MTPS.Core;

public abstract class BaseFeatureWithAwaiters : BaseFeature
{
    protected bool IsRunning;
    public override void OnEnterState()
    {
        IsRunning = true;
    }

    public override void OnExitState()
    {
        IsRunning = false;
    }
}