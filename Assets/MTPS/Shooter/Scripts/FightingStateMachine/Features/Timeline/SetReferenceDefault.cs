using MTPS.Core;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;
using Object = UnityEngine.Object;

public class SetReferenceDefault : BaseFeatureWithAwaiters
{
    [SerializeField] private string outPutname;
    [SerializeField] protected Object value;
    
    private AnimationLayer _layer;
    protected IFightingStateMachineVariables _variables;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _variables = variables as IFightingStateMachineVariables;
        _layer = _variables.AnimationLayer;
    }

    public override async void OnEnterState()
    {
        base.OnEnterState();
        await _layer.WaitForNextState();
        if(!IsRunning) return;
        if (value == null) 
            value = _variables.weaponInstance.GetComponent<Animator>();
        _layer.CurrentGraph.SetReference(outPutname,value);
    }
}