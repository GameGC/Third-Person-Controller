using MTPS.Core;

namespace MTPS.Shooter.FightingStateMachine.Features
{
    public class SetWeaponActiveFeature : BaseFeature
    {
        public bool active;
    
        private IFightingStateMachineVariables _variables;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
        }

        public override void OnEnterState()
        {
            _variables.weaponInstance.SetActive(active);
        
            if(_variables.secondaryWeaponInstance)
                _variables.secondaryWeaponInstance.SetActive(active);
        }
    }
}
