using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    public Transform cameraPivot; // Reference to CameraRoot
    public float mouseSensitivity = 3f;
    public float clampAngle = 80f;

    private PlayerInputActions inputActions;
    private Vector2 lookInput;
    private float yaw;
    private float pitch;

    void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => lookInput = Vector2.zero;
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void LateUpdate() {
        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -clampAngle, clampAngle);

        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
