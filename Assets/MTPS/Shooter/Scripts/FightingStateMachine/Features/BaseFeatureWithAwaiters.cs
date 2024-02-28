using MTPS.Core;

namespace MTPS.Shooter.FightingStateMachine.Features
{
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
}