using MTPS.Core;
using UnityEngine;

namespace MTPS.Movement.Core
{
    public interface IMoveStateMachineVariables : IStateMachineVariables
    { 
        // readonly Variables 
        public float MovementSmooth { get; }
        public LayerMask GroundLayer { get; }
   
   
        //dynamic Variables
        public bool IsGrounded { get; set; }
        public RaycastHit GroundHit { get; set; }
        
        public bool IsSlopeBadForMove { get; set; }
        public float SlopeAngle { get; set; }
   
        public bool JumpCounterElapsed  { get; set; }
        public float MoveSpeed { get; set; }
    }
}