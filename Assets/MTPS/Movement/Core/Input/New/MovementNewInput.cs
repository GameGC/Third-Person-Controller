#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace MTPS.Movement.Core.Input.New
{
    public class MovementNewInput : MonoBehaviour, IMoveInput , InputActions.IMovementActions
    {
        protected static InputActions InputActions;
        
        private Transform _cameraMain;

        private void Awake() => InputActions ??= new InputActions();

        private void Start()
        {
            _cameraMain = Camera.main.transform;
            
            var cameraObject = FindObjectsOfType<CameraInputProvider>(true);
            foreach (var cameraInputProvider in cameraObject) 
                cameraInputProvider.InputReader = this;
        }

        protected virtual void OnEnable()
        {
            InputActions.Movement.AddCallbacks(this);
            InputActions.Enable();
        }

        protected virtual void OnDisable()
        {
            InputActions.Movement.RemoveCallbacks(this);
            InputActions.Disable();
        }


        void InputActions.IMovementActions.OnMove(InputAction.CallbackContext obj)
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

        void InputActions.IMovementActions.OnLook(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            lookInput = obj.ReadValue<Vector2>();
        }


        void InputActions.IMovementActions.OnJump(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            isJump = obj.action.IsPressed();
        }

        void InputActions.IMovementActions.OnSprint(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            isSprinting = obj.action.IsPressed();
        }

        void InputActions.IMovementActions.OnCrouch(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            if (obj.action.WasPerformedThisFrame())
                isCrouch = !isCrouch;
        }

        void InputActions.IMovementActions.OnProne(InputAction.CallbackContext obj)
        {
            if (isInputFrozen) return;
            if (obj.action.WasPerformedThisFrame())
                isProne = !isProne;
        }

        private void Update()
        {
            if (isInputFrozen) return; 

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
                    var newDirection = moveInputSmooth.x * right + moveInputSmooth.y * forward;
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
        
        public Vector2 lookInput { get; private set; }

        public Quaternion cameraRotation => _cameraMain.rotation;

        public Vector2 moveInput { get; private set; }
        public Vector2 moveInputSmooth { get; private set; }

        public float moveInputMagnitude { get; set; }
        public Vector3 moveDirection { get; private set; }

        public bool isSprinting { get; private set; }
        public bool isJump { get; private set; }

        public bool isCrouch { get; private set; }
        public bool isProne { get; private set; }
        public bool isRoll { get; }
        
        public IMoveInput.MoveToPointDelegate MoveToPoint { get; set; }

        [field: SerializeField] public float movementSmooth { get; set; } = 6;

        [field:SerializeField]
        public bool isInputFrozen { get; set; }
    }
}
#endif