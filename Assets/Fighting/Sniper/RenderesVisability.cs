using System;
using JetBrains.Annotations;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

public class RenderesVisability : BaseFeature
{
    public bool isActiveDuringState;
    
    private Renderer[] _renderers;
    private IFightingStateMachineVariables _variables;
    public override void CacheReferences([NotNull] IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _variables = variables as IFightingStateMachineVariables;
        _renderers = resolver.GetComponent<Transform>().GetComponentsInChildren<Renderer>();
    }

    public override void OnEnterState()
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = isActiveDuringState;
        }

        foreach (var componentsInChild in _variables.weaponInstance.GetComponentsInChildren<Renderer>())
        {
            componentsInChild.enabled = isActiveDuringState;
        }
    }

    public override void OnExitState()
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = !isActiveDuringState;
        }
        
        foreach (var componentsInChild in _variables.weaponInstance.GetComponentsInChildren<Renderer>())
        {
            componentsInChild.enabled = !isActiveDuringState;
        }
    }
}
