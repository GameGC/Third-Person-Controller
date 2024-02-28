using MTPS.Core;
using MTPS.Shooter.WeaponsUI;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Features.Scope
{
    public class SetScopeActiveFeature : BaseFeature
    {
        [SerializeField] private bool _active;

        private PlayerHUD _hud;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _hud = resolver.GetNamedComponent<PlayerHUD>("PlayerHUD");
        }

        public override void OnEnterState()
        {
            if(_hud)
                _hud.SetScopeActive(_active);
        }
    }
}