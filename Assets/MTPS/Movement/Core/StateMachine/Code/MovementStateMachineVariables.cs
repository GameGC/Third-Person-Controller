using UnityEngine;

namespace MTPS.Movement.Core.StateMachine
{
    [DisallowMultipleComponent]
    public class MovementStateMachineVariables : MonoBehaviour,IMoveStateMachineVariables
    {
        [field: SerializeField][field: Range(1f, 20f)]
        public float MovementSmooth { get; private set; } = 6f;

        [Tooltip("Layers that the character can walk on")]
        [field: SerializeField]
        public LayerMask GroundLayer { get; private set; } = 1;

        public bool IsGrounded { get; set; }
        public RaycastHit GroundHit { get; set; }
        public bool IsSlopeBadForMove { get; set; }
        public float SlopeAngle { get; set; }
        public bool JumpCounterElapsed { get; set; } = true;

        public bool DonTAddGravity { get; set; } = false;

        public float MoveSpeed { get; set; }
    }
}
