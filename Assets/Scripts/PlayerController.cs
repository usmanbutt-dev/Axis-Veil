using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

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
    }

    void Update() {
        // Nothing in Update for now
    }

    void FixedUpdate() {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 worldMove = transform.TransformDirection(move) * moveSpeed;
        rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);

        if (jumpPressed && isGrounded) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, isInverted ? -jumpForce : jumpForce, rb.linearVelocity.z);
        }

        jumpPressed = false; // reset after applying
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
