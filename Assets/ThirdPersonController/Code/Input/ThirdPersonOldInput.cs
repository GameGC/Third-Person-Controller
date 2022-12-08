using System;
using ThirdPersonController.MovementStateMachine;
using UnityEngine;

namespace ThirdPersonController.Input
{
    public sealed class ThirdPersonOldInput : BaseInputReader
    {
        [Header("Legacy refs")] 
        [SerializeField] private MovementStateMachineVariables cc;
        #region Variables       

        [Header("Controller _input")]
        [SerializeField] private string horizontalInput = "Horizontal";
        [SerializeField] private string verticallInput = "Vertical";
        [SerializeField] private KeyCode jumpInput = KeyCode.Space;
        [SerializeField] private KeyCode sprintInput = KeyCode.LeftShift;

        [Header("Camera _input")]
        [SerializeField] private string rotateCameraXInput = "Mouse X";
        [SerializeField] private string rotateCameraYInput = "Mouse Y";

        private Transform _cameraMain;

        #endregion

        
        private void Update()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            JumpInput();

            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
                isCrouch = !isCrouch;
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
                isProne = !isProne;
        }


        #region Basic Locomotion Inputs

       

        public void MoveInput()
        {
            moveInput.x = UnityEngine.Input.GetAxis(horizontalInput);
            moveInput.y = UnityEngine.Input.GetAxis(verticallInput);
            
            moveInputMagnitude = Mathf.Max(Mathf.Abs(moveInput.x), Mathf.Abs(moveInput.y));
            
            // calculate moveInput smooth
            moveInputSmooth = Vector2.MoveTowards(moveInputSmooth, moveInput, cc.MovementSmooth * Time.deltaTime);
        }

        private void CameraInput()
        {
            if (!_cameraMain)
            {
                if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                else
                {
                    _cameraMain = Camera.main.transform;
                }
            }

            if (_cameraMain)
            {
                UpdateMoveDirection();
            }

          

            lookInput.y =  UnityEngine.Input.GetAxis(rotateCameraYInput);
            lookInput.x  = UnityEngine.Input.GetAxis(rotateCameraXInput);
        }

        private void UpdateMoveDirection()
        {
            if (moveInput.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero,  cc.MovementSmooth * Time.deltaTime);
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


        private void SprintInput()
        {
            if (UnityEngine.Input.GetKeyDown(sprintInput))
                isSprinting = SprintConditions();
            else if (UnityEngine.Input.GetKeyUp(sprintInput))
                isSprinting = false;
        }

     
        private bool SprintConditions()
        {
            return moveInput.sqrMagnitude > 0.1f;
        }
        
        /// <summary>
        /// _input to trigger the Jump 
        /// </summary>
        private void JumpInput()
        {
            if (UnityEngine.Input.GetKeyDown(jumpInput))
                isJump = true;
            else
            {
                isJump = false;
            }
        }

        #endregion       
    }
}