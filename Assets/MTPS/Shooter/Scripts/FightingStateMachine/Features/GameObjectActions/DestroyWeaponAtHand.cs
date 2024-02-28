using System;
using GameGC.CommonEditorUtils.Attributes;
using MTPS.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MTPS.Shooter.FightingStateMachine.Features.GameObjectActions
{
    [Serializable]
    public class DestroyWeaponAtHand : BaseFeature
    {
        [SerializeField,ClipToSeconds] private float delay = 1;
    
        private IFightingStateMachineVariables _variables;
    
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
        }

        public override void OnEnterState()
        {
            Object.Destroy(_variables.weaponInstance,delay);
        
            //if weapon is 2 handed
            if(_variables.secondaryWeaponInstance)
                Object.Destroy(_variables.secondaryWeaponInstance,delay);
        }
    }
}