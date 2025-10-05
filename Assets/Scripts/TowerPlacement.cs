using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TowerPlacement : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Tilemap tilemap;
    public GameObject towerPrefab;
    public GameObject previewPrefab;
    public BillboardSprite billboardManager; // ← reference to your billboard manager

    [Header("Preview Colors")]
    public Color validColor = new(0f, 1f, 0f, 0.6f);
    public Color invalidColor = new(1f, 0f, 0f, 0.6f);

    private GameObject previewTower;
    private Renderer previewRenderer;
    private readonly Dictionary<Vector3Int, bool> occupiedTiles = new();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (tilemap == null)
        {
            Debug.LogError("Assign a Tilemap to TowerPlacementAuto.");
            enabled = false;
            return;
        }

        // Create preview
        if (previewPrefab != null)
        {
            previewTower = Instantiate(previewPrefab);
            Collider col = previewTower.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            previewRenderer = previewTower.GetComponentInChildren<Renderer>();
        }
    }

    void Update()
    {
        if (mainCamera == null || tilemap == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Plane plane = new(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out float enter)) return;
        Vector3 world = ray.GetPoint(enter);

        Vector3Int cell = tilemap.WorldToCell(world);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cell);

        bool occupied = occupiedTiles.ContainsKey(cell) && occupiedTiles[cell];

        // Update preview
        if (previewTower != null)
        {
            previewTower.transform.position = cellCenter;
            if (previewRenderer != null)
                previewRenderer.material.color = occupied ? invalidColor : validColor;
        }

        // Place tower
        if (Mouse.current.leftButton.wasPressedThisFrame && !occupied)
        {
            GameObject newTower = Instantiate(towerPrefab, cellCenter, Quaternion.identity);
            occupiedTiles[cell] = true;

            // Automatically register to billboard manager
            if (billboardManager != null)
                billboardManager.RegisterSprite(newTower.transform);
        }
    }
}