using System;
using System.Linq;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public class MultipleConditionTransition : BaseStateTransition
{
    [Header("When call conditions true")]
    
    
    [SerializeReference,SerializeReferenceAddButton(typeof(BaseStateTransition))]
    public BaseStateTransition[] Transitions = new BaseStateTransition[0];

    public void OnValidate()
    {
        for (var i = 0; i < Transitions.Length; i++)
        {
            Transitions[i].path = $"{path}.Transitions.Array.data[{i}]";
        }

    }
    
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        foreach (var transition in Transitions)
        {
            transition.Initialise(variables,resolver);
        }
    }

    public override bool couldHaveTransition 
    {
        get { return Transitions.All(t => t.couldHaveTransition); }
    }
}