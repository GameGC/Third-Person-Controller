using ThirdPersonController.Core.DI;

namespace ThirdPersonController.Code.AnimatedStateMachine
{
    public interface IFightingStateMachineVariables : IStateMachineVariables
    {
        public bool couldAttack { get; set; }
        public bool isCooldown { get; set; }
        public bool isReloading { get; set; }
    }
}