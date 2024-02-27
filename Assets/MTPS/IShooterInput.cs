using MTPS.Core;

namespace ThirdPersonController.Input
{
    public interface IShooterInput  : IBaseInputReader
    {
        public bool IsAttack { get; set; }
        public bool IsAim  { get; set; }
    }
}