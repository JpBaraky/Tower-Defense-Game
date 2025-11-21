using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Area preview that follows the mouse in world XZ plane and draws a circle using a LineRenderer.
/// Use Initialize(radius, groundY) to configure. Calls OnConfirm(center) on left click.
/// </summary>
public class AreaTargetPreview : MonoBehaviour
{
    public Action<Vector3> OnConfirm;

    private LineRenderer lr;
    private float radius = 2f;
    private int segments = 64;
    private float groundY = 0.1f; // Y plane where circle sits
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;

        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;      // we set world positions directly
        lr.loop = true;
        lr.widthMultiplier = 0.07f;
        lr.positionCount = segments;
        lr.material = new Material(Shader.Find("Unlit/Color")); // simple fallback
        lr.material.color = new Color(1f, 0f, 0f, 0.6f);
    }

    /// <summary>
    /// Call to configure the preview before use.
    /// groundY = the Y coordinate where the circle should lie (usually 0).
    /// </summary>
    public void Initialize(float r, float groundY = 0.1f, int segmentCount = 64)
    {
        radius = Mathf.Max(0.01f, r);
        this.groundY = groundY;
        segments = Mathf.Clamp(segmentCount, 8, 512);
        lr.positionCount = segments;
        UpdateCirclePositions(transform.position); // initial
    }

    void Update()
    {
        UpdatePosition();
        DetectConfirm();
    }

    private void UpdatePosition()
    {
        Vector3 mouseWorld = GetMouseToPlanePoint(groundY);
        if (mouseWorld == Vector3.positiveInfinity) return;

        transform.position = mouseWorld;
        UpdateCirclePositions(mouseWorld);
    }

    private void DetectConfirm()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnConfirm?.Invoke(transform.position);
            Destroy(gameObject);
        }

        // cancel with right click (optional)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Destroy(gameObject);
        }
    }

    // Computes world positions for the circle points (XZ plane, at given center)
    private void UpdateCirclePositions(Vector3 center)
    {
        float stepDeg = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float deg = stepDeg * i;
            float rad = deg * Mathf.Deg2Rad;

            // XZ plane (Y is groundY)
            float x = Mathf.Cos(rad) * radius;
            float z = Mathf.Sin(rad) * radius;

            Vector3 worldPos = new Vector3(center.x + x, groundY, center.z + z);
            lr.SetPosition(i, worldPos);
        }
    }

    // Projects mouse to a horizontal plane at y = planeY using a camera ray
    private Vector3 GetMouseToPlanePoint(float planeY)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return Vector3.positiveInfinity;

        Vector2 mouse = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mouse);

        Plane plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.positiveInfinity;
    }
}
