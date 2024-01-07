using System.Threading.Tasks;
using UnityEngine;

namespace ThirdPersonController.Input
{
    public interface IBaseInputReader
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
        
        
        public bool IsAttack { get; set; }
        public bool IsAim  { get; set; }

        public delegate Task MoveToPointDelegate(Pose point);
        
        public MoveToPointDelegate MoveToPoint { get; set; }
    }
}