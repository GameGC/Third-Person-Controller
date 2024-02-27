using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core.Input;

public class ProneTransition : BaseStateTransition
{
    public bool isProne;

    private IMoveInput _inputReader;
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _inputReader = resolver.GetComponent<IMoveInput>();
    }

    public override bool couldHaveTransition => _inputReader.isProne == isProne;
}
