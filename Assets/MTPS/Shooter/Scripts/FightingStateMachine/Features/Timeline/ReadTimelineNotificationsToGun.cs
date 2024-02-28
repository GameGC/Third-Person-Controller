using MTPS.Core;
using UnityEngine.Playables;

namespace MTPS.Shooter.FightingStateMachine.Features.Timeline
{
    public class ReadTimelineNotificationsToGun : BaseFeatureWithAwaiters
    {
        public string outputName ="Signal Track";
        private INotificationReceiver _receiver;
    
        private AnimationLayer layer;
        private IFightingStateMachineVariables _variables;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IFightingStateMachineVariables;
            layer = _variables.AnimationLayer;
        }

        public override async void OnEnterState()
        {
            base.OnEnterState();
            await layer.WaitForNextState();
            if(!IsRunning) return;
            if(_receiver == null)
                _receiver = _variables.weaponInstance.GetComponent<INotificationReceiver>();
            layer.CurrentGraph.SubscribeNotification(outputName,_receiver);
        }

        public override void OnExitState()
        {
            base.OnExitState();
            layer.CurrentGraph.UnSubscribeNotification(outputName,_receiver);
        }
    }
}