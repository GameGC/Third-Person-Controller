#if ENABLE_INPUT_SYSTEM

using ThirdPersonController.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MTPS.Movement.Core.Input.New
{

    public class ShooterNewInput : MovementNewInput , IShooterInput , InputActions.IShooterActions
    {
        protected override void OnEnable()
        {
            InputActions.Shooter.AddCallbacks(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            InputActions.Shooter.RemoveCallbacks(this);
            base.OnDisable();
        }

        void InputActions.IShooterActions.OnAim(InputAction.CallbackContext obj)
        {
            // Shooting stripped — aim disabled
        }
        void InputActions.IShooterActions.OnAttack(InputAction.CallbackContext obj)
        {
            // Shooting stripped — attack disabled
        }

        void InputActions.IShooterActions.OnLongShoot(InputAction.CallbackContext context)
        {
        }

        [field:SerializeField]
        public bool IsAttack { get; set; }
        
        [field:SerializeField]
        public bool IsAim  { get; set; }
    }


}
#endif
