using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPersonController.Input
{
#if ENABLE_INPUT_SYSTEM

    public sealed class ThirdPersonNewInput : BaseInputReader
    { 
        private Transform _cameraMain;

        private void Awake()
        {
            _cameraMain = Camera.main.transform;
        }
        
        
        private void OnMove(InputValue obj)
        {
            moveInput = obj.Get<Vector2>();
            moveInputMagnitude = Mathf.Max(Mathf.Abs(moveInput.x), Mathf.Abs(moveInput.y));
         
        }
        private void OnLook(InputValue obj)
        {
            lookInput = obj.Get<Vector2>();
        }
        
        
        private void OnJump(InputValue obj) => isJump = obj.isPressed;

        private void OnSprint(InputValue obj) => isSprinting = obj.isPressed;

        private void OnCrouch(InputValue obj) => isCrouch = !isCrouch;

        private void OnProne(InputValue obj) => isProne = !isProne;


        private void Update()
        {
            UpdateMoveDirection();

            
            // calculate moveInput smooth
            moveInputSmooth = Vector2.MoveTowards(moveInputSmooth, moveInput, movementSmooth * Time.deltaTime);
        }


        private void UpdateMoveDirection()
        {
            if (moveInput.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero,  movementSmooth * Time.deltaTime);
                return;
            }

            if (_cameraMain)
            {
                //get the right-facing direction of the referenceTransform
                var rotation = _cameraMain.rotation;
                
                var right = rotation * Vector3.right;
                var forward = rotation * Vector3.forward;
                
                // determine the direction the player will face based on moveInput and the referenceTransform's right and forward directions
                moveDirection = (moveInputSmooth.x * right) + (moveInputSmooth.y * forward);
                
                moveDirection.y = 0;
                moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);
                moveDirection.z = Mathf.Clamp(moveDirection.z, -1f, 1f);
            }
            else
            {
                moveDirection = new Vector3(moveInputSmooth.x, 0, moveInputSmooth.y);
            }
        }
    }
#endif

}