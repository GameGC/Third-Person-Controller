#if ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine;

namespace MTPS.Movement.Core.Input.Old
{
    public class MovementOldInput : MonoBehaviour, IMoveInput
    {
        #region Variables

        [Header("Controller Move")] [SerializeField]
        private string horizontalInput = "Horizontal";

        [SerializeField] private string verticalInput = "Vertical";
        [SerializeField] private KeyCode jumpInput = KeyCode.Space;
        [SerializeField] private KeyCode sprintInput = KeyCode.LeftShift;

        [Header("Camera Look")] [SerializeField]
        private string rotateCameraXInput = "Mouse X";

        [SerializeField] private string rotateCameraYInput = "Mouse Y";

        private Transform _cameraMain;

        #endregion

        private void Start()
        {
            _cameraMain = Camera.main.transform;
        }

        protected virtual void Update()
        {
            if (isInputFrozen)
            {
                moveInput = Vector2.zero;
                lookInput = Vector2.zero;
                moveInputMagnitude = 0;
                return;
            }

            MoveInput();
            CameraInput();
            SprintInput();
            JumpInput();

            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
                isCrouch = !isCrouch;

            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
                isProne = !isProne;

            moveInputSmooth = Vector2.MoveTowards(moveInputSmooth, moveInput, movementSmooth * Time.deltaTime);
        }


        #region Basic Locomotion Inputs

        private void MoveInput()
        {
            moveInput = new Vector2(UnityEngine.Input.GetAxis(horizontalInput), UnityEngine.Input.GetAxis(verticalInput));
            moveInputMagnitude = Mathf.Max(Mathf.Abs(moveInput.x), Mathf.Abs(moveInput.y));

            // calculate moveInput smooth
            moveInputSmooth = Vector2.MoveTowards(moveInputSmooth, moveInput, movementSmooth * Time.deltaTime);
        }

        private void CameraInput()
        {
            UpdateMoveDirection();

            lookInput = new Vector2(UnityEngine.Input.GetAxis(rotateCameraXInput), UnityEngine.Input.GetAxis(rotateCameraYInput));
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

            if (moveInput.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, movementSmooth * Time.deltaTime);
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
            return moveInputMagnitude > 0.1f;
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

        [field: SerializeField] public bool isInputFrozen { get; set; }
    }
}
#endif