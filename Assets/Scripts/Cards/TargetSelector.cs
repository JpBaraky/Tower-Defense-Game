using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class TargetSelector : MonoBehaviour
{
    public static TargetSelector Instance;

    public LineRenderer line;
    public Transform arrowStart;
    public Action<Enemy> onTargetSelected;

    private Camera cam;
    private bool active;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;

        if (line != null)
            line.positionCount = 2;
    }

    void Update()
    {
        if (!active) return;

        Vector3 mouseWorld = GetMouseWorld();
        line.SetPosition(0, arrowStart.position);
        line.SetPosition(1, mouseWorld);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Enemy hit = TryGetEnemyUnderMouse();
            onTargetSelected?.Invoke(hit);
            StopSelecting();
        }
    }

    public void BeginSelecting(Transform start, Action<Enemy> callback)
    {
        arrowStart = start;
        onTargetSelected = callback;
        active = true;
        line.enabled = true;
    }

    private void StopSelecting()
    {
        active = false;
        line.enabled = false;
    }

   private Vector3 GetMouseWorld()
{
    if (cam == null) cam = Camera.main;

    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

    // Adjust plane height to your world (Y = 0 for ground)
    Plane plane = new Plane(Vector3.up, Vector3.zero);

    if (plane.Raycast(ray, out float dist))
        return ray.GetPoint(dist);

    return Vector3.zero;
}

    private Enemy TryGetEnemyUnderMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
            return hit.collider.GetComponentInParent<Enemy>();

        return null;
    }
}
