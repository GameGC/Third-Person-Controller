﻿using System.Threading.Tasks;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;

namespace Fighting.Pushing
{
    public class FightingStateMachineVariables : MonoBehaviour,IFightingStateMachineVariables
    {
        public GameObject weaponInstance { get; set; }
        
        public bool couldAttack { get; set; } = true;
        public bool isCooldown { get; set; }
        public bool isReloading { get; set; }
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

    public class ShootFeature : BaseFeature
    {
        private GunShootingInfo _shooter;
        private IFightingStateMachineVariables _variables;

        
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables= variables as IFightingStateMachineVariables;
        }

        public override void OnEnterState()
        {
            if (!_shooter)
                _shooter = _variables.weaponInstance.GetComponent<GunShootingInfo>();
            
            //wait until animation apply
            DelayShoot();
        }

        private async void DelayShoot()
        {
            await Task.Delay(1000);
            _shooter.Shoot();
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