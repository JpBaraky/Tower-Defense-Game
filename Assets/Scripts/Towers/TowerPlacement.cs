using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TowerPlacement : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Tilemap tilemap;
    public Tilemap pathTilemap;
    public Tilemap groundTilemap;
    public GameObject towerPrefab;
    public BillboardSprite billboardManager;

    [Header("Economy")]
    public int startingGold = 100; 
    public TextMeshProUGUI goldDisplay;
    public int currentGold;
    [Range(0f, 1f)] public float towerSellMultiplier = 0.5f;

    [Header("Preview Colors")]
    public Color validColor = new Color(0f, 1f, 0f, 0.6f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.6f);
    [Range(0f, 5f)] public float pulseSpeed = 2f;
    [Range(0f, 1f)] public float pulseStrength = 0.3f;

    public GameObject previewTower;
    private Renderer[] previewRenderers;
    private readonly Dictionary<Vector3Int, bool> occupiedTiles = new();
    private readonly Dictionary<Vector3Int, bool> isGroundTile = new();

    private bool canPlaceTower;
    private bool changedPosition = true;
    private Vector3Int lastCell;

    void Start()
    {
        currentGold = startingGold;
        if (mainCamera == null) mainCamera = Camera.main;

        if (tilemap == null)
        {
            Debug.LogError("Assign a Tilemap to TowerPlacement.");
            enabled = false;
            return;
        }

        UpdatePreview();
        MarkPathOnSmallTiles();
        MarkGround();
    }

    void Update()
    {
        goldDisplay.text = currentGold.ToString();

        if (!canPlaceTower)
        {
            if (previewRenderers != null)
                foreach (var rend in previewRenderers) rend.enabled = false;
            return;
        }
        else
        {
            if (previewRenderers != null)
                foreach (var rend in previewRenderers) rend.enabled = true;
        }

        if (Keyboard.current.shiftKey.wasReleasedThisFrame)
            canPlaceTower = false;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // Try a physics hit first (hit a collider under the cursor).
        // If that fails, fall back to the X/Z from the camera-plane intersection (preserves previous behavior for tiles without colliders).
        int hitMask = LayerMask.GetMask("Ground", "Default");
        bool hasMouseHit = Physics.Raycast(ray, out RaycastHit mouseHit, 200f, hitMask, QueryTriggerInteraction.Ignore);
        Vector3 mouseHitPoint;

        if (hasMouseHit)
        {
            mouseHitPoint = mouseHit.point;
        }
        else
        {
            // fallback to plane intersection to get X/Z coordinates for tile mapping (keeps placement working on tiles without colliders like path tiles)
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(ray, out float enter))
                return;
            mouseHitPoint = ray.GetPoint(enter);
        }

        // Use the X/Z of the hit (or plane) to map to the tile cell. Zero the Y for the tilemap lookup.
        Vector3 worldOnPlane = new Vector3(mouseHitPoint.x, 0f, mouseHitPoint.z);
        Vector3Int cell = tilemap.WorldToCell(worldOnPlane);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cell);

        bool occupied = occupiedTiles.ContainsKey(cell) && occupiedTiles[cell];
        bool isGround = isGroundTile.ContainsKey(cell) && isGroundTile[cell];

        if (previewTower != null)
        {
            if (cell != lastCell)
            {
                changedPosition = true;
                lastCell = cell;
            }

            if (changedPosition)
            {
                // If we had a physics hit we can start the downward sample from the hit point's Y.
                // If not, start from a fixed height above tile center so path tiles (no collider) also get a measured top Y.
                float sampleStartY = hasMouseHit ? (mouseHitPoint.y + 1f) : (cellCenter.y + 20f);
                float groundY = GetStableGroundHeight(new Vector3(cellCenter.x, sampleStartY, cellCenter.z));
                previewTower.transform.position = new Vector3(cellCenter.x, groundY, cellCenter.z);
                changedPosition = false;
            }

            if (previewRenderers != null)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * pulseStrength;

                Color baseColor;
                // Show preview on path or other tiles, but mark invalid visually;
                // occupied prevents placement but the preview still appears.
                if (occupied || !isGround)
                    baseColor = invalidColor;
                else
                {
                    int towerPrice = towerPrefab.GetComponent<TowerPrice>().price;
                    baseColor = (currentGold >= towerPrice) ? validColor : invalidColor;
                }

                Color pulsingColor = baseColor * (1f + pulse);
                pulsingColor.a = baseColor.a;

                foreach (var rend in previewRenderers)
                    if (rend != null && rend.material != null)
                        rend.material.color = pulsingColor;
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !occupied && isGround)
        {
            int towerPrice = towerPrefab.GetComponent<TowerPrice>().price;

            if (currentGold >= towerPrice)
            {
                currentGold -= towerPrice;
              
                Vector3 towerPos = previewTower != null ? previewTower.transform.position : new Vector3(cellCenter.x, GetStableGroundHeight(cellCenter), cellCenter.z);
                GameObject newTower = Instantiate(towerPrefab, towerPos, Quaternion.identity, transform);
                occupiedTiles[cell] = true;

            }
            else
            {
                Debug.Log("Not enough gold to place tower.");
            }

            if (!Keyboard.current.shiftKey.isPressed || Keyboard.current.shiftKey.wasReleasedThisFrame)
                canPlaceTower = false;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            // Use the same raycast approach to find a tower under the cursor for selling.
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, hitMask, QueryTriggerInteraction.Ignore))
            {
                TowerPrice tower = hit.collider.GetComponent<TowerPrice>();
                if (tower != null)
                {
                    int refund = Mathf.RoundToInt(tower.price * towerSellMultiplier);
                    currentGold += refund;
                    Debug.Log($"Sold tower for {refund} gold. Current gold: {currentGold}");

                    Vector3Int towerCell = tilemap.WorldToCell(tower.transform.position);
                    occupiedTiles[towerCell] = false;

                    Destroy(tower.gameObject);
                }
            }
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
            Destroy(obj);

        previewTower = Instantiate(towerPrefab);
        previewTower.name = "PreviewTower";
        previewTower.tag = "Preview";
    foreach (Transform t in previewTower.GetComponentsInChildren<Transform>(true))
    t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        Collider col = previewTower.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        previewRenderers = previewTower.GetComponentsInChildren<Renderer>();
        foreach (var rend in previewRenderers)
        {
            if (rend == null) continue;
            rend.material = new Material(rend.material);
            rend.enabled = false;
        }

        previewTower.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);

        TowerTargeting targeting = previewTower.GetComponent<TowerTargeting>() ??
                                   previewTower.GetComponentInChildren<TowerTargeting>();
        if (targeting != null)
            targeting.isPreview = true;
    }

    // Casts a short down-ray from above the provided X/Z and returns the top surface Y (so the tower sits on top).
    // Caller should set fromPosition.y to a value above the expected surface.
    float GetStableGroundHeight(Vector3 fromPosition)
    {
        const float downDistance = 60f;
        int mask = LayerMask.GetMask("Ground", "Default");
        Ray down = new Ray(fromPosition, Vector3.down);

        if (Physics.Raycast(down, out RaycastHit hit, downDistance, mask, QueryTriggerInteraction.Ignore))
            return hit.point.y;

        return 0f;
    }

    void MarkPathOnSmallTiles()
    {
        foreach (var bigCell in pathTilemap.cellBounds.allPositionsWithin)
        {
            if (pathTilemap.GetTile(bigCell) == null) continue;

            Vector3 worldCenter = pathTilemap.GetCellCenterWorld(bigCell);
            float bigHexSize = pathTilemap.layoutGrid.cellSize.x * 0.5f;
            Bounds hexBounds = new Bounds(worldCenter, Vector3.one * bigHexSize * 2f);

            Vector3Int minCell = tilemap.WorldToCell(hexBounds.min);
            Vector3Int maxCell = tilemap.WorldToCell(hexBounds.max);

            for (int x = minCell.x; x <= maxCell.x; x++)
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    Vector3Int smallCell = new Vector3Int(x, y, 0);
                    Vector3 cellCenter = tilemap.GetCellCenterWorld(smallCell);
                    if (Vector3.Distance(cellCenter, worldCenter) <= bigHexSize * 0.95f)
                        occupiedTiles[smallCell] = true;
                }
        }
    }

    void MarkGround()
    {
        foreach (var bigCell in groundTilemap.cellBounds.allPositionsWithin)
        {
            if (groundTilemap.GetTile(bigCell) == null) continue;

            Vector3 worldCenter = groundTilemap.GetCellCenterWorld(bigCell);
            float bigHexSize = groundTilemap.layoutGrid.cellSize.x * 0.5f;
            Bounds hexBounds = new Bounds(worldCenter, Vector3.one * bigHexSize * 2f);

            Vector3Int minCell = tilemap.WorldToCell(hexBounds.min);
            Vector3Int maxCell = tilemap.WorldToCell(hexBounds.max);

            for (int x = minCell.x; x <= maxCell.x; x++)
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    Vector3Int smallCell = new Vector3Int(x, y, 0);
                    Vector3 cellCenter = tilemap.GetCellCenterWorld(smallCell);
                    if (Vector3.Distance(cellCenter, worldCenter) <= bigHexSize * 1f)
                        isGroundTile[smallCell] = true;
                }
        }
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
    }
    // Call from other scripts to free the tile a tower was occupying
public void FreeCellAtPosition(Vector3 worldPosition)
{
    // Map world X/Z to the tile cell used by placement
    Vector3 worldOnPlane = new Vector3(worldPosition.x, 0f, worldPosition.z);
    Vector3Int cell = tilemap.WorldToCell(worldOnPlane);
    FreeCell(cell);
}

public void FreeCell(Vector3Int cell)
{
    if (occupiedTiles.ContainsKey(cell))
    {
        occupiedTiles[cell] = false;
        // If you prefer to remove the entry entirely, use:
        // occupiedTiles.Remove(cell);
    }
    else
    {
        // Optional: ensure the dict has a clear entry for this cell
        // occupiedTiles[cell] = false;
    }
}
}
