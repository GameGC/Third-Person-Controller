using System.Linq;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class BaseRigFeature : BaseFeatureWithAwaiters
{
    //[SerializeField] protected string waitForStateWeight;
    [SerializeField] private string layerName;
    [SerializeField] private bool returnPreviousValueOnExit = true;
    
    protected RigLayer _targetLayer;
    private float _previousValue;

    protected AnimationLayer _animationLayer;
    
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        //if (!string.IsNullOrEmpty(waitForStateWeight))
        _animationLayer = (variables as IFightingStateMachineVariables).AnimationLayer;

        _targetLayer = resolver.GetComponent<RigBuilder>().layers.
            FirstOrDefault(l => l.name == layerName);
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        _previousValue = _targetLayer.rig.weight;
    }

    public override void OnExitState()
    {
        base.OnExitState();
        if (returnPreviousValueOnExit)
            _targetLayer.rig.weight = _previousValue;
    }
}