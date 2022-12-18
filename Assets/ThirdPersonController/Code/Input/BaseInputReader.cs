using UnityEngine;

namespace ThirdPersonController.Input
{
    public class BaseInputReader : MonoBehaviour
    {
        public Vector2 lookInput       ;
        
        public Vector2 moveInput       ;
        public Vector2 moveInputSmooth ;
        
        public float moveInputMagnitude;
        public Vector3 moveDirection   ;
        
        public bool isSprinting        ;
        public bool isJump             ;
        
        public bool isCrouch           ;
        public bool isProne            ;
        public bool isRoll             ;

        
        public float movementSmooth { protected get; set; }
    }
}