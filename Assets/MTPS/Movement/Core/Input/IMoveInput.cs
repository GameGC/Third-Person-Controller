using System.Threading.Tasks;
using MTPS.Core;
using UnityEngine;

namespace MTPS.Movement.Core.Input
{
    public interface IMoveInput : IBaseInputReader
    {
        public Vector2 lookInput       {get;}
        public Quaternion cameraRotation { get; }


        public Vector2 moveInput       {get;}
        public Vector2 moveInputSmooth {get;}
        
        public float moveInputMagnitude{get; set; }
        public Vector3 moveDirection   {get;}
        
        public bool isSprinting        {get;}
        public bool isJump             {get;}
        
        public bool isCrouch           {get;}
        public bool isProne            {get;}
        public bool isRoll             {get;}

        public float movementSmooth { get; set; }
        
        
        public bool isInputFrozen { get; set; }
        
        public delegate Task MoveToPointDelegate(Pose point);
        
        public MoveToPointDelegate MoveToPoint { get; set; }
    }
}