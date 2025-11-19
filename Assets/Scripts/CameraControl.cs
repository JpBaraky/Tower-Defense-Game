using UnityEngine;
using UnityEngine.InputSystem;

public class TowerDefenseCamera : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float fastSpeed = 60f;
    public float smoothTime = 0.1f;

    [Header("Zoom")]
    public float zoomSpeed = 50f;
    public Vector2 zoomLimits = new Vector2(10f, 80f);

    [Header("Map Bounds")]
    public Vector2 minBounds = new Vector2(-50, -50);
    public Vector2 maxBounds = new Vector2(50, 50);

    [Header("Isometric Angle")]
    public float pitch = 45f;
    public float yaw = 45f;

    [Header("Mouse Drag")]
    public float dragSpeed = 1f;

    [Header("Edge Scrolling")]
    public bool edgeScroll = true;
    public float edgeScrollSpeed = 20f;
    public float edgeSize = 10f; // pixels from screen edge

    private Vector3 targetPosition;
    private Vector3 velocity;

    private Vector2 moveInput;
    private float zoomInput;

    private bool isDragging = false;
    private Vector2 lastMousePosition;

    void Start()
    {
        targetPosition = transform.position;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void Update()
    {
      
   
        HandleKeyboardMovement();
        HandleMouseDrag();
        HandleEdgeScroll();
        HandleZoom();
        ClampTargetPosition();
        SmoothMove();
    }

    #region Input Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        zoomInput = context.ReadValue<float>();
    }
    #endregion

    void HandleKeyboardMovement()
    {
        float speed = Keyboard.current.leftShiftKey.isPressed ? fastSpeed : moveSpeed;

        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;
        targetPosition += moveDir * speed * Time.deltaTime;
    }

void HandleMouseDrag()
{
    if (Mouse.current.rightButton.wasPressedThisFrame)
    {
        isDragging = true;
        lastMousePosition = Mouse.current.position.ReadValue();
    }
    else if (Mouse.current.rightButton.wasReleasedThisFrame)
    {
        isDragging = false;

        // Reset SmoothDamp velocity so camera stops immediately
        velocity = Vector3.zero;
    }

    if (isDragging)
    {
        Vector2 mouseDelta = Mouse.current.position.ReadValue() - lastMousePosition;

        // Drag opposite to mouse for “grab and pull” feel
        Vector3 dragMovement = new Vector3(-mouseDelta.x, 0f, -mouseDelta.y) * dragSpeed * Time.deltaTime;

        // Optional: scale drag by camera height
        dragMovement *= transform.position.y / 20f;

        targetPosition += dragMovement;
        lastMousePosition = Mouse.current.position.ReadValue();
    }
}

void HandleEdgeScroll()
{
    if (!edgeScroll) return;

    Vector3 edgeMovement = Vector3.zero;
    Vector2 mousePos = Mouse.current.position.ReadValue();

    if (mousePos.x <= edgeSize) edgeMovement += Vector3.left;
    else if (mousePos.x >= Screen.width - edgeSize) edgeMovement += Vector3.right;

    if (mousePos.y <= edgeSize) edgeMovement += Vector3.back;
    else if (mousePos.y >= Screen.height - edgeSize) edgeMovement += Vector3.forward;

    if (edgeMovement != Vector3.zero)
    {
        edgeMovement.Normalize();
        targetPosition += edgeMovement * edgeScrollSpeed * Time.deltaTime;
    }
}
    void HandleZoom()
    {
        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            Vector3 zoomDir = transform.forward * zoomInput * zoomSpeed * Time.deltaTime;
            targetPosition += zoomDir;
            zoomInput = 0f;
        }
    }

    void ClampTargetPosition()
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
        targetPosition.y = Mathf.Clamp(targetPosition.y, zoomLimits.x, zoomLimits.y);
    }

    void SmoothMove()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
