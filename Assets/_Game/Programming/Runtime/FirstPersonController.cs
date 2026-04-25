// FirstPersonController.cs
// Requires: Unity Input System package, CharacterController component
// Setup: Attach to a GameObject with a CharacterController. 
//        Create an InputActionAsset with "Move" (Vector2), "Look" (Vector2), and "Jump" (Button) actions.

using Linework.EdgeDetection;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -19.62f; // 2x gravity for snappier feel

    [Header("Look")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float verticalLookClamp = 85f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;

    // Components
    private CharacterController _controller;
    private PlayerInput _playerInput;

    // Input Actions
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;

    // State
    private Vector3 _velocity;
    private float _cameraPitch;
    private bool _isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // Grab actions by name from the PlayerInput component's action asset
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _jumpAction = _playerInput.actions["Jump"];
        _sprintAction = _playerInput.actions["Sprint"];

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        GroundCheck();
        HandleGravity();
        HandleMovement();
        HandleJump();
        HandleLook();
    }

    // -------------------------------------------------------------------------
    // Ground Detection
    // -------------------------------------------------------------------------

    private void GroundCheck()
    {
        _isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        // Bleed off accumulated downward velocity when grounded
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    // -------------------------------------------------------------------------
    // Gravity
    // -------------------------------------------------------------------------

    private void HandleGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private void HandleMovement()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        bool isSprinting = _sprintAction.IsPressed();

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Build a direction relative to where the player is facing
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        _controller.Move(move * (currentSpeed * Time.deltaTime));
    }

    // -------------------------------------------------------------------------
    // Jump
    // -------------------------------------------------------------------------

    private void HandleJump()
    {
        if (_jumpAction.WasPressedThisFrame() && _isGrounded)
        {
            // v = sqrt(h * -2 * g)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // -------------------------------------------------------------------------
    // Mouse Look
    // -------------------------------------------------------------------------

    private void HandleLook()
    {
        Vector2 lookDelta = _lookAction.ReadValue<Vector2>();

        // Horizontal — rotate the whole player body
        transform.Rotate(Vector3.up * lookDelta.x * mouseSensitivity);

        // Vertical — rotate only the camera (pitch)
        _cameraPitch -= lookDelta.y * mouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -verticalLookClamp, verticalLookClamp);
        cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    // -------------------------------------------------------------------------
    // Editor Helpers
    // -------------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
