using Fighting.Pushing;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine.Playables;

public class ReadTimelineNotificationsToGun : BaseFeature
{
    public string outputName ="Signal Track";
    private INotificationReceiver _receiver;
    
    private AnimationLayer layer;
    private IFightingStateMachineVariables _variables;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _variables = variables as IFightingStateMachineVariables;
        layer = (variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
    }

    public override void OnEnterState()
    {
        if(_receiver == null)
            _receiver = _variables.weaponInstance.GetComponent<INotificationReceiver>();
        layer.Graph.SubscribeNotification(outputName,_receiver);
    }

    public override void OnExitState()
    {
        layer.Graph.UnSubscribeNotification(outputName,_receiver);
    }
}