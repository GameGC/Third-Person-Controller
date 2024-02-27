using System.Linq;
using System.Threading.Tasks;
using GameGC.CommonEditorUtils.Attributes;
using MTPS.Core;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Fighting.Pushing
{
    public class ShootFeature : BaseFeatureWithAwaiters
    {
        [SerializeField] private bool changeCooldownToAwait = true;
        
        [SerializeField] private bool setAimingWeaigh0DuringShoot = true;
        [SerializeField,ClipToSeconds] private float shootAnimationLength;
        
        private IWeaponInfo _shooter;
        private IFightingStateMachineVariables _variables;

        private MultiAimConstraint _handAimConstaint;
        private RigBuilder _rigBuilder;

        private bool _wasCooldown;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables= variables as IFightingStateMachineVariables;
            _rigBuilder = resolver.GetComponent<RigBuilder>();

            if (_rigBuilder && _rigBuilder.layers[(int)RigTypes.Fighting].rig)
            {
               
            }
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            _shooter ??= _variables.weaponInstance.GetComponent<IWeaponInfo>();

            if (!_handAimConstaint)
            {
                var constraints = _rigBuilder.layers[(int) RigTypes.Fighting].constraints;
                if (constraints == null || constraints.Length < 1) return;
                //foreach (var rigConstraint in constraints)
                //{
                //    if(rigConstraint is MultiAimConstraint) 
                //        if(rigConstraint.component.name ==  "HandAim") 
                //        Debug.Log((rigConstraint as Component).name);
                //}
                var handAim = constraints.FirstOrDefault(
                    c => (c.component is MultiAimConstraint) && c.component.name == "HandAim");
                if (handAim != null)
                    _handAimConstaint = handAim as MultiAimConstraint;
            }

            //wait until animation apply
            DelayShoot();
        }

        private async void DelayShoot()
        {
            _wasCooldown = _variables.isCooldown;
            if(changeCooldownToAwait)
                _variables.isCooldown = true;
            var animationController = _variables.AnimationLayer;
            await animationController.WaitForNextState();

            //immediate shoot fix bug
            _rigBuilder.Evaluate(0);
            await Task.Yield();

            if(!IsRunning) return;
            if(setAimingWeaigh0DuringShoot)
                if (_handAimConstaint)
                {
                    _handAimConstaint.weight = 0;
#pragma warning disable CS4014
                    Task.Delay((int) (shootAnimationLength * 1000)).ContinueWith(ResetWeight);
#pragma warning restore CS4014
                }

            if(changeCooldownToAwait)
                _variables.isCooldown = _wasCooldown;
            _shooter.Shoot();
            //if(setAimingWeaigh0DuringShoot)
            //    if (_handAimConstaint)
            //    {
            //        await Task.Yield();
            //        _handAimConstaint.weight = 1;
            //    }
        }

        private void ResetWeight(Task task)
        {
            _handAimConstaint.weight = 1;
        }
    }
}