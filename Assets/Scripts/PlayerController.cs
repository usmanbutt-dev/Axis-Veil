using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float rotationSpeed = 5f;
    public Transform cameraTransform; // Reference to the camera transform

    private PlayerInputActions inputActions;
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool jumpPressed = false;
    private bool isGrounded = false;
    private bool isMoving = false;

    void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.Move.performed += ctx => {
            moveInput = ctx.ReadValue<Vector2>();
            isMoving = moveInput.magnitude > 0.1f;
        };
        inputActions.Player.Move.canceled += _ => {
            moveInput = Vector2.zero;
            isMoving = false;
        };
        inputActions.Player.Jump.performed += _ => jumpPressed = true;
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void FixedUpdate() {
        // Only align with camera when moving
        if (isMoving) {
            // Get camera forward direction, ignoring vertical component
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Movement relative to camera direction
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 worldMove = cameraTransform.TransformDirection(move) * moveSpeed;
        worldMove.y = 0; // Ensure we don't move vertically
        rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);

        // Jumping
        if (jumpPressed && isGrounded) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
    }

    void OnCollisionStay(Collision collision) {
        // Check if we're standing on something
        foreach (ContactPoint contact in collision.contacts) {
            if (contact.normal.y > 0.7f) {
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