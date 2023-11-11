using System;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;

public interface IHealthIStateMachineVariables : IStateMachineVariables
{
    public float health { get; set; }
    public event Action OnHealthChanged;
}

public class HealthLowTransition : BaseStateTransition
{
    public float healthToTransition;
    
    private IHealthIStateMachineVariables _variables;

    private bool _couldHaveTransition;

    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _variables = variables as IHealthIStateMachineVariables;
        _variables.OnHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged()
    {
        _couldHaveTransition = _variables.health <= healthToTransition;
    }

    public override bool couldHaveTransition => _couldHaveTransition;
}