using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TowerPlacement : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Tilemap tilemap;         // Main tilemap for tower placement
    public Tilemap pathTilemap;     // Tilemap that holds the path
    public GameObject towerPrefab;
    public BillboardSprite billboardManager; // reference to your billboard manager

    [Header("Preview Colors")]
    public Color validColor = new Color(0f, 1f, 0f, 0.6f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.6f);

    public GameObject previewTower;
    private Renderer previewRenderer;
    private readonly Dictionary<Vector3Int, bool> occupiedTiles = new();
    private bool canPlaceTower;
  

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (tilemap == null)
        {
            Debug.LogError("Assign a Tilemap to TowerPlacement.");
            enabled = false;
            return;
        }

        UpdatePreview();
        MarkPathOnSmallTiles();
    }

    void Update()
    {

        if (!canPlaceTower)
        {
            if (previewRenderer != null) previewRenderer.enabled = false;
            return;
        }
        else
        {
            if (previewRenderer != null) previewRenderer.enabled = true;
        }
        if (Keyboard.current.shiftKey.wasReleasedThisFrame)
        {
            canPlaceTower = false;
        }
      
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        Plane plane = new(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out float enter)) return;

        Vector3 world = ray.GetPoint(enter);
        Vector3Int cell = tilemap.WorldToCell(world);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cell);

        bool occupied = occupiedTiles.ContainsKey(cell) && occupiedTiles[cell];

        // Check if this cell exists in the path tilemap
        // Mark all small tiles overlapping the path as occupied

        // Update preview color
        if (previewTower != null)
        {
            previewTower.transform.position = cellCenter;
            if (previewRenderer != null)
                previewRenderer.material.color = (occupied) ? invalidColor : validColor;
        }

        // Place tower only if the cell is free and not a path
        if (Mouse.current.leftButton.wasPressedThisFrame && !occupied)
        {
            GameObject newTower = Instantiate(towerPrefab, cellCenter, Quaternion.identity);
            if (!Keyboard.current.shiftKey.isPressed || Keyboard.current.shiftKey.wasReleasedThisFrame)
{
    canPlaceTower = false;
}
            
                occupiedTiles[cell] = true;

                if (billboardManager != null)
                    billboardManager.RegisterSprite(newTower.transform);
            
        }
    }

    public void CanPlaceTower()
    {
        UpdatePreview();
        canPlaceTower = true;
    }

    public void UpdatePreview()
    { 
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Preview"))
{
    Destroy(obj);
}      

        if(previewTower != towerPrefab)
        {
            previewTower = towerPrefab;
        }
        {

        }
            previewTower = Instantiate(towerPrefab);
            previewTower.name = "PreviewTower";
                previewTower.tag = "Preview";
                Collider col = previewTower.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                previewRenderer = previewTower.GetComponentInChildren<Renderer>();
                previewTower.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
                if (previewRenderer != null) previewRenderer.enabled = false;
            
        
    }
   // Call in Start()
// Call this in Start() to mark all small tiles under path hexes
void MarkPathOnSmallTiles()
{
    foreach (var bigCell in pathTilemap.cellBounds.allPositionsWithin)
    {
        if (pathTilemap.GetTile(bigCell) == null) continue;

        Vector3 worldCenter = pathTilemap.GetCellCenterWorld(bigCell);
        float bigHexSize = pathTilemap.layoutGrid.cellSize.x * 0.5f; // approximate hex radius
        Bounds hexBounds = new Bounds(worldCenter, Vector3.one * bigHexSize * 2f);

        // Determine range in small tile cells to check
        Vector3Int minCell = tilemap.WorldToCell(hexBounds.min);
        Vector3Int maxCell = tilemap.WorldToCell(hexBounds.max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int smallCell = new(x, y, 0);
                Vector3 cellCenter = tilemap.GetCellCenterWorld(smallCell);

                // Only mark if inside the big hex radius
                if (Vector3.Distance(cellCenter, worldCenter) <= bigHexSize * 0.95f)
                    occupiedTiles[smallCell] = true;
            }
        }
    }
}

}
