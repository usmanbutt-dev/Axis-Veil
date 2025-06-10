using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public Transform cameraTransform; // Reference to the main camera's transform

    private PlayerInputActions inputActions;
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool jumpPressed = false;
    private bool isGrounded = false;
    private bool isInverted = false;
    private Vector3 gravityDirection = Vector3.down;

    void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += _ => jumpPressed = true;
        inputActions.Player.Invert.performed += _ => InvertGravity();
    }

    void OnEnable() {
        inputActions.Enable();
    }

    void OnDisable() {
        inputActions.Disable();
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        Physics.gravity = gravityDirection * 9.81f;

        // If cameraTransform isn't set, try to find the main camera
        if (cameraTransform == null) {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update() {
        // Make the player face the camera's forward direction (but keep upright)
        if (cameraTransform != null) {
            // Get camera's forward direction, but ignore pitch (up/down) rotation
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0; // Keep the player upright
            cameraForward.Normalize();

            if (cameraForward != Vector3.zero) {
                transform.forward = cameraForward;
            }
        }
    }

    void FixedUpdate() {
        // Calculate movement relative to camera
        Vector3 move = Vector3.zero;
        if (cameraTransform != null) {
            // Get camera's forward and right vectors (ignoring pitch)
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();

            // Combine input with camera orientation
            move = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
        }
        else {
            // Fallback to local movement if no camera
            move = new Vector3(moveInput.x, 0f, moveInput.y);
        }

        // Apply movement
        Vector3 worldMove = move * moveSpeed;
        rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);

        if (jumpPressed && isGrounded) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, isInverted ? -jumpForce : jumpForce, rb.linearVelocity.z);
            jumpPressed = false;
        }
    }

    void InvertGravity() {
        isInverted = !isInverted;
        gravityDirection = isInverted ? Vector3.up : Vector3.down;
        Physics.gravity = gravityDirection * 9.81f;
        transform.Rotate(180f, 0f, 0f);
    }

    void OnCollisionStay(Collision collision) {
        foreach (ContactPoint contact in collision.contacts) {
            if (Vector3.Dot(contact.normal, gravityDirection) < -0.5f) {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    void OnCollisionExit(Collision collision) {
        isGrounded = false;
    }
}