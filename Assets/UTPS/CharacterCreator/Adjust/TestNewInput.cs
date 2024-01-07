using System;
using System.Collections.Generic;
using ThirdPersonController.Input;
using ThirdPersonController.Input.New;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestNewInput : MonoBehaviour, IBaseInputReader
{
    public InputActionAsset inputActions;
    [SerializeField] private bool _isAim;

    private void Start()
    {
        var cameraObject = FindObjectsOfType<CameraInputProvider>(true);
        foreach (var cameraInputProvider in cameraObject)
            cameraInputProvider._inputReader = this;

        var Player = inputActions.FindActionMap("Player", throwIfNotFound: true);
        Player.Enable();

        var tempArray = new[]
        {
            new KeyValuePair<string, Action<InputAction.CallbackContext>>("Look", OnLook),
        };

        foreach (var keyValuePair in tempArray)
        {
            var action = Player.FindAction(keyValuePair.Key, true);
            action.performed += keyValuePair.Value;
            action.canceled += keyValuePair.Value;
            action.started += keyValuePair.Value;
            action.Enable();
        }
            
#if UNITY_EDITOR
        Keyboard.current.leftCommandKey.pressPoint =0.0001f;
        Keyboard.current.fKey.pressPoint =0.0001f;
#endif
    }
   
    private void OnLook(InputAction.CallbackContext obj)
    {
        if (isInputFrozen) return;
        lookInput = obj.ReadValue<Vector2>();
    }
   
    public Vector2 lookInput { get; private set; }
   
    public Quaternion cameraRotation { get; }
    public Vector2 moveInput { get; }
    public Vector2 moveInputSmooth { get; }
    public float moveInputMagnitude { get; set; }
    public Vector3 moveDirection { get; }
    public bool isSprinting { get; }
    public bool isJump { get; }
    public bool isCrouch { get; }
    public bool isProne { get; }
    public bool isRoll { get; }
    public float movementSmooth { get; set; }
    public bool isInputFrozen { get; set; }
    public bool IsAttack { get; set; }

    public bool IsAim
    {
        get => _isAim;
        set => _isAim = value;
    }

    public IBaseInputReader.MoveToPointDelegate MoveToPoint { get; set; }
}