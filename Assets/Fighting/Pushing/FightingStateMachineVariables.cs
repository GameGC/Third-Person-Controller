using System.Threading.Tasks;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Fighting.Pushing
{
    public class FightingStateMachineVariables : MonoBehaviour,IFightingStateMachineVariables
    {
        public GameObject weaponInstance { get; set; }
        
        public bool couldAttack { get; set; } = true;
        public bool isCooldown { get; set; }
        public bool isReloading { get; set; }
        public bool RequestedHolsterWeapon  { get; set; }
    }
    
    public class HolsterTransition : BaseStateTransition
    {
        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables= variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => _variables.RequestedHolsterWeapon;
    }
    public class AttackTransition : BaseStateTransition
    {
        [SerializeField] private bool isAttacking;

        private IBaseInputReader _input;
        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
            _variables= variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition
        {
            get
            {
                if (_input.IsAttack && isAttacking && _variables.couldAttack) return true;
                else return _input.IsAttack == isAttacking;
            }
        }
    }
    
    public class LongShootTransition : BaseStateTransition
    {
        [SerializeField] private InputActionReference shootButtonReference;

        private IBaseInputReader _input;
        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
            _variables= variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition
        {
            get
            {
                if (shootButtonReference.action.triggered)
                {
                    Debug.Log("sadagf");
                    return true;
                }
                return false;
            }
        }
    }

    public class ShootFeature : BaseFeature
    {
        private IWeaponInfo _shooter;
        private IFightingStateMachineVariables _variables;

        private MultiAimConstraint _handAimConstaint;
        
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables= variables as IFightingStateMachineVariables;
            _handAimConstaint = resolver.GetNamedComponent<MultiAimConstraint>("HandAim");
        }

        public override void OnEnterState()
        {
            _shooter ??= _variables.weaponInstance.GetComponent<IWeaponInfo>();

            //wait until animation apply
            DelayShoot();
        }

        private async void DelayShoot()
        {
            var animationController = (_variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
            await animationController.WaitForStateWeight1("Shoot");
            _handAimConstaint.weight = 0;
            _shooter.Shoot();
        }

        public override void OnExitState()
        {
            _handAimConstaint.weight = 1;
        }
    }
    
    
    
    public class AimTransition : BaseStateTransition
    {
        [SerializeField] private bool isAim;

        private IBaseInputReader _input;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IBaseInputReader>();
        }

        public override bool couldHaveTransition => _input.IsAim == isAim;
    }
    
    
    
    public class CooldownTransition : BaseStateTransition
    {
        [SerializeField] private bool isCooldown;

        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => _variables.isCooldown == isCooldown;
    }
    
       
    public class ReloadingTransition : BaseStateTransition
    {
        [SerializeField] private bool isReloading;

        private IFightingStateMachineVariables _variables;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
        }

        public override bool couldHaveTransition => _variables.isReloading == isReloading;
    }
}