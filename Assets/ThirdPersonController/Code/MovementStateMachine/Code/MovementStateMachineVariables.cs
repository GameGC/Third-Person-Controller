using UnityEngine;

namespace ThirdPersonController.MovementStateMachine
{
    public class MovementStateMachineVariables : MonoBehaviour,IMoveStateMachineVariables
    {
        [field: SerializeField][field: Range(1f, 20f)]
        public float MovementSmooth { get; private set; } = 6f;

        [Tooltip("Layers that the character can walk on")]
        [field: SerializeField]
        public LayerMask GroundLayer { get; private set; } = 1;

        public bool IsGrounded { get; set; }
        public float GroundDistance { get; set; }
        public bool IsSlopeBadForMove { get; set; }
        public float SlopeAngle { get; set; }
        public bool JumpCounterElapsed { get; set; } = true;
        public float MoveSpeed { get; set; }
    }
}
