using System.Linq;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine.Animations.Rigging;

namespace Fighting.Pushing
{
    public class ShootFeature : BaseFeatureWithAwaiters
    {
        public bool changeCooldownToAwait = true;
        public bool setAimingWeaigh0DuringShoot = true;
        // public string waitForStateWeightName = "Shoot";
        
        private IWeaponInfo _shooter;
        private IFightingStateMachineVariables _variables;

        private MultiAimConstraint _handAimConstaint;
        private RigBuilder _rigBuilder;

        private bool wasCooldown;
        private bool firstShoot = true;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables= variables as IFightingStateMachineVariables;
            _rigBuilder = resolver.GetComponent<RigBuilder>();

            if (_rigBuilder && _rigBuilder.layers.Count > 0 && _rigBuilder.layers[0].rig)
            {
                var handAim = _rigBuilder.layers[0].constraints.FirstOrDefault(
                    c => (c.component is MultiAimConstraint) && c.component.name == "HandAim");
                if (handAim != null)
                    _handAimConstaint = handAim as MultiAimConstraint;
            }
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            _shooter ??= _variables.weaponInstance.GetComponent<IWeaponInfo>();

            //wait until animation apply
            DelayShoot();
        }

        private async void DelayShoot()
        {
            wasCooldown = _variables.isCooldown;
            if(changeCooldownToAwait)
                _variables.isCooldown = true;
            var animationController = (_variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
            await animationController.WaitForNextState();

            //immediate shoot fix bug
            _rigBuilder.Evaluate(0);

            if(!IsRunning) return;
            if(setAimingWeaigh0DuringShoot)
                _handAimConstaint.weight = 0;
            
            if(changeCooldownToAwait)
                _variables.isCooldown = wasCooldown;
            _shooter.Shoot();
            if(setAimingWeaigh0DuringShoot)
                _handAimConstaint.weight = 1;
        }
    }
}