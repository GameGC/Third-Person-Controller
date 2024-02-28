using MTPS.Core;
using MTPS.Shooter.WeaponsUI;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Features.Scope
{
    public class SetSniperScopeActiveFeature : BaseFeature
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
                _hud.SetFullScreenScopeActive(_active);
        }
    }
}