using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPersonController.Input
{
#if ENABLE_INPUT_SYSTEM

    public sealed class ThirdPersonNewInput : BaseInputReader
    {
        [SerializeField] private InputActionAsset inputActions;
        private Transform _cameraMain;

        
        private void Awake()
        {
            _cameraMain = Camera.main.transform;
            
            var Player = inputActions.FindActionMap("Player", throwIfNotFound: true);
            Player.Enable();
            
            var tempArray = new[]
            {
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Move",OnMove), 
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Look", OnLook),
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Jump",OnJump),
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Sprint",OnSprint),
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Crouch",OnCrouch),
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Prone",OnProne),
            };
            
            foreach (var keyValuePair in tempArray)
            {
                var action = Player.FindAction(keyValuePair.Key, true);
                action.performed += keyValuePair.Value;
                action.canceled += keyValuePair.Value;
                action.started += keyValuePair.Value;
                action.Enable();
            }
        }
        
        
        private void OnMove(InputAction.CallbackContext obj)
        {
            moveInput = obj.ReadValue<Vector2>();
            moveInputMagnitude = Mathf.Max(Mathf.Abs(moveInput.x), Mathf.Abs(moveInput.y));
         
        }
        private void OnLook(InputAction.CallbackContext obj)
        {
            lookInput = obj.ReadValue<Vector2>();
        }
        
        
        private void OnJump(InputAction.CallbackContext obj) => isJump = obj.action.IsPressed();

        private void OnSprint(InputAction.CallbackContext obj) => isSprinting = obj.action.IsPressed();

        private void OnCrouch(InputAction.CallbackContext obj)
        {
            if(obj.action.WasPerformedThisFrame())
                isCrouch = !isCrouch;
        }

        private void OnProne(InputAction.CallbackContext obj)
        {
            if(obj.action.WasPerformedThisFrame())
                isProne = !isProne;
        }


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

        private void OnDisable()
        {
            inputActions.Disable();

            foreach (var assetActionMap in inputActions.actionMaps)
            {
                assetActionMap.Disable();
                assetActionMap.Dispose();
            }

        }
    }
#endif

}