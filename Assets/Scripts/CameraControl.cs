using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour

{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float fastSpeed = 60f;
    public float smoothTime = 0.1f;

    [Header("Rotation")]
    public float rotateSpeed = 200f;
    public float minPitch = -90f;
    public float maxPitch = 90f;
    public float rotationSmoothTime = 0.05f;

    [Header("Zoom / Fly Height")]
    public float zoomSpeed = 50f;

    [Header("Map Bounds")]
    public Vector2 minBounds = new Vector2(-50, -50); // X,Z
    public Vector2 maxBounds = new Vector2(50, 50);   // X,Z
    public Vector2 heightLimits = new Vector2(5f, 100f); // Y min/max

    private Vector2 moveInput;
    private Vector2 rotateInput;
    private float zoomInput;

    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;

    private float pitch = 0f;
    private float yaw = 0f;
    private float targetPitch;
    private float targetYaw;

    void Start()
    {
        targetPosition = transform.position;
        Vector3 angles = transform.eulerAngles;
        yaw = targetYaw = angles.y;
        pitch = targetPitch = angles.x;
    }

    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            HandleRotation();
            HandleMovement();
        }

        HandleZoom();
        ClampTargetPosition();
        SmoothUpdate();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotateInput = context.ReadValue<Vector2>();
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        zoomInput = context.ReadValue<float>();
    }

    void HandleMovement()
    {
        float speed = Keyboard.current.leftShiftKey.isPressed ? fastSpeed : moveSpeed;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.Normalize();
        right.Normalize();

        Vector3 dir = forward * moveInput.y + right * moveInput.x;
        targetPosition += dir * speed * Time.deltaTime;
    }

    void HandleRotation()
    {
        targetYaw += rotateInput.x * rotateSpeed * Time.deltaTime;
        targetPitch -= rotateInput.y * rotateSpeed * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }

    void HandleZoom()
    {
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            targetPosition += transform.forward * zoomInput * zoomSpeed * Time.deltaTime;
            zoomInput = 0f;
        }
    }

    void ClampTargetPosition()
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
        targetPosition.y = Mathf.Clamp(targetPosition.y, heightLimits.x, heightLimits.y);
    }

    void SmoothUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        yaw = Mathf.LerpAngle(yaw, targetYaw, rotationSmoothTime);
        pitch = Mathf.Lerp(pitch, targetPitch, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}