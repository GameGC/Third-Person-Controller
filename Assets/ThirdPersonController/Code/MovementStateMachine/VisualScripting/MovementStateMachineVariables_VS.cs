
using ThirdPersonController.MovementStateMachine;
using Unity.VisualScripting;
using UnityEngine;

[IncludeInSettings(true)]
public class MovementStateMachineVariables_VS : Variables,IMoveStateMachineVariables
{
    public float MovementSmooth   { get => declarations.Get<float>(nameof(MovementSmooth));    private set => declarations.Set(nameof(MovementSmooth),value); }
    public LayerMask GroundLayer  { get => declarations.Get<LayerMask>(nameof(GroundLayer));    private set => declarations.Set(nameof(GroundLayer),value); }

    public bool IsGrounded        { get => declarations.Get<bool>(nameof(IsGrounded));         set => declarations.Set(nameof(IsGrounded),value); }
    public float GroundDistance   { get => declarations.Get<float>(nameof(GroundDistance));    set => declarations.Set(nameof(GroundDistance),value); }
    public bool IsSlopeBadForMove { get => declarations.Get<bool>(nameof(IsSlopeBadForMove));  set => declarations.Set(nameof(IsSlopeBadForMove),value); }
    public float SlopeAngle       { get => declarations.Get<float>(nameof(SlopeAngle));        set => declarations.Set(nameof(SlopeAngle),value); }
    public bool JumpCounterElapsed{ get => declarations.Get<bool>(nameof(JumpCounterElapsed)); set => declarations.Set(nameof(JumpCounterElapsed),value); }
    public float MoveSpeed        { get => declarations.Get<float>(nameof(MoveSpeed));         set => declarations.Set(nameof(MoveSpeed),value); }

    private void Reset() => OnValidate();

    private void OnValidate()
    {
        MovementSmooth = 6;
        GroundLayer = 1;

        IsGrounded = true;
        GroundDistance = 0;
        IsSlopeBadForMove = false;
        SlopeAngle = 0;
        JumpCounterElapsed = true;
        MoveSpeed = 0;
    }
}
