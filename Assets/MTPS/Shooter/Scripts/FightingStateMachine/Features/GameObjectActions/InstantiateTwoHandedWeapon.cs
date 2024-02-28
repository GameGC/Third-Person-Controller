using System;
using MTPS.Core;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MTPS.Shooter.FightingStateMachine.Features.GameObjectActions
{
    [Serializable]
    public class InstantiateTwoHandedWeapon : BaseFeature
    {
        [SerializeField] private GameObject leftPrefab;
        [SerializeField] private GameObject rightPrefab;
    
        private Transform _leftHand;
        private Transform _rightHand;

    
        private IFightingStateMachineVariables _variables;
        private IReferenceResolver _resolver;

        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            var animator = resolver.GetComponent<Animator>();
            _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        
            _variables = variables as IFightingStateMachineVariables;
            _resolver = resolver;
        }

        public override void OnEnterState()
        {
            IWeaponInfo info;
        
            var instance = Object.Instantiate(leftPrefab, _leftHand,false);
            _variables.secondaryWeaponInstance = instance;
        
            if (instance.TryGetComponent(out info)) 
                info.CacheReferences(_variables,_resolver);
        
            instance = Object.Instantiate(rightPrefab, _rightHand,false);
            _variables.weaponInstance = instance;
        
            if (instance.TryGetComponent(out info)) 
                info.CacheReferences(_variables,_resolver);
        }
    }
}