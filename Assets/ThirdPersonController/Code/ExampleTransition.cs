using System.Collections;
using System.Collections.Generic;
using StateMachineLogic.DI;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.MovementStateMachine.Code.Transitions;
using UnityEngine;

public class ExampleTransition : BaseStateTransition
{

    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        throw new System.NotImplementedException();
    }

    public override bool couldHaveTransition { get; }
}
