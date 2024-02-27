#if ENABLE_LEGACY_INPUT_MANAGER
using MTPS.Movement.Core.Input.Old;
using UnityEngine;


namespace ThirdPersonController.Input
{
    using Input = UnityEngine.Input;

    public class ShooterOldInput : MovementOldInput,IShooterInput
    {
        [Header("Fighting")]
        [SerializeField] private string attackAxis = "Fire1";
        [SerializeField] private string aimAxis = "Fire2";
        
        protected override void Update()
        {
            base.Update();
            if (isInputFrozen) return;

            AttackInput();
            AimInput();
        }

        private void AttackInput() => IsAttack = Input.GetAxis(attackAxis) > 0;

        private void AimInput() => IsAim = Input.GetAxis(aimAxis) > 0;

        [field:SerializeField]
        public bool IsAttack { get; set; }
        
        [field:SerializeField]
        public bool IsAim  { get; set; }
    }
}

#endif
