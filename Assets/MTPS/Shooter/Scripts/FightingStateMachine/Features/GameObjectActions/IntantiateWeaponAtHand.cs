using System;
using MTPS.Core;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MTPS.Shooter.FightingStateMachine.Features.GameObjectActions
{
    [Serializable]
    public class IntantiateWeaponAtHand : BaseFeature
    {
        [SerializeField] private GameObject prefab;
    
        private Animator _animator;
        private IFightingStateMachineVariables _variables;
        private IReferenceResolver _resolver;

        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _animator = resolver.GetComponent<Animator>();
            _variables = variables as IFightingStateMachineVariables;
            _resolver = resolver;
        }

        public override void OnEnterState()
        {
            var instance = Object.Instantiate(prefab, _animator.GetBoneTransform(HumanBodyBones.RightHand),false);
            _variables.weaponInstance = instance;
        
            if (instance.TryGetComponent<IWeaponInfo>(out var info)) 
                info.CacheReferences(_variables,_resolver);
        }
    }
}