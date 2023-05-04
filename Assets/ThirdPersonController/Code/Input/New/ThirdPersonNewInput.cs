using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using ThirdPersonController.Input.New;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPersonController.Input
{
#if ENABLE_INPUT_SYSTEM

    public sealed class ThirdPersonNewInput : MonoBehaviour, IBaseInputReader
    {
        public InputActionAsset inputActions;
        private Transform _cameraMain;


        private void Start()
        {
            _cameraMain = Camera.main.transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var cameraObject = FindObjectOfType<CameraInputProvider>();
            cameraObject._inputReader = this;

            var freeLook = cameraObject.GetComponent<CinemachineFreeLook>();
            freeLook.Follow = transform;
            freeLook.LookAt = transform;

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
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Attack",OnAttack),
                new KeyValuePair<string, Action<InputAction.CallbackContext>>("Aim",OnAim),
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
            if (isInputFrozen)
            {
                moveInput = Vector2.zero;
                moveInputMagnitude = 0;
                return;
            }
            moveInput = obj.ReadValue<Vector2>();
            moveInputMagnitude = Mathf.Max(Mathf.Abs(moveInput.x), Mathf.Abs(moveInput.y));

        }
        private void OnLook(InputAction.CallbackContext obj)
        {
            lookInput = obj.ReadValue<Vector2>();
        }


        private void OnJump(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            isJump = obj.action.IsPressed();
        }

        private void OnSprint(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            isSprinting = obj.action.IsPressed();
        }

        private void OnCrouch(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            if (obj.action.WasPerformedThisFrame())
                isCrouch = !isCrouch;
        }

        private void OnProne(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            if (obj.action.WasPerformedThisFrame())
                isProne = !isProne;
        }

        private void OnAim(InputAction.CallbackContext obj)
        {
            IsAim = obj.action.IsPressed();
        }
        
        private void OnAttack(InputAction.CallbackContext obj)
        {
            IsAttack = obj.action.IsPressed();
        }


        private void Update()
        {
            UpdateMoveDirection();


            // calculate moveInput smooth
            moveInputSmooth = Vector2.MoveTowards(moveInputSmooth, moveInput, movementSmooth * Time.deltaTime);
        }


        private void UpdateMoveDirection()
        {
            if (moveInputMagnitude < 0.02f)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, movementSmooth * Time.deltaTime);
                return;
            }
            else
            {
                if (_cameraMain)
                {
                    //get the right-facing direction of the referenceTransform
                    var rotation = _cameraMain.rotation;

                    var right = rotation * Vector3.right;
                    var forward = rotation * Vector3.forward;

                    // determine the direction the player will face based on moveInput and the referenceTransform's right and forward directions
                    var newDirection = (moveInputSmooth.x * right) + (moveInputSmooth.y * forward);
                    newDirection.y = 0;
                    newDirection.x = Mathf.Clamp(newDirection.x, -1f, 1f);
                    newDirection.z = Mathf.Clamp(newDirection.z, -1f, 1f);

                    moveDirection = newDirection;
                }
                else
                {
                    moveDirection = new Vector3(moveInputSmooth.x, 0, moveInputSmooth.y);
                }
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
        public Vector2 lookInput { get; protected set; }

        public Quaternion cameraRotation => _cameraMain.rotation;

        public Vector2 moveInput { get; protected set; }
        public Vector2 moveInputSmooth { get; protected set; }

        public float moveInputMagnitude { get; set; }
        public Vector3 moveDirection { get; protected set; }

        public bool isSprinting { get; protected set; }
        public bool isJump { get; protected set; }

        public bool isCrouch { get; protected set; }
        public bool isProne { get; protected set; }
        public bool isRoll { get; protected set; }

        public bool IsAttack { get; set; }
        public bool IsAim  { get; set; }
        
        [field: SerializeField] public float movementSmooth { get; set; } = 6;

        
        public bool isInputFrozen { get; set; }
    }
#endif

}