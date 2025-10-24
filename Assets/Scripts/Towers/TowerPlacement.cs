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
    private bool canPlaceTower;
    private readonly Dictionary<Vector3Int, bool> isGroundTile = new();

    void Start()
    {
        currentGold = startingGold;
        Debug.Log("Starting Gold: " + currentGold);

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
        Plane plane = new(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out float enter)) return;

        Vector3 world = ray.GetPoint(enter);
        Vector3Int cell = tilemap.WorldToCell(world);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cell);

        bool occupied = occupiedTiles.ContainsKey(cell) && occupiedTiles[cell];
        bool isGroundTile = this.isGroundTile.ContainsKey(cell) && this.isGroundTile[cell];

        // Update preview color
        if (previewTower != null)
        {
            
            previewTower.transform.position = cellCenter;

            if (previewRenderers != null)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * pulseStrength;
              
                Color baseColor;
                if (occupied || !isGroundTile)
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

        // Place tower
        if (Mouse.current.leftButton.wasPressedThisFrame && !occupied && isGroundTile)
        {
            int towerPrice = towerPrefab.GetComponent<TowerPrice>().price;

            if (currentGold >= towerPrice)
            {
                currentGold -= towerPrice;
                Debug.Log("Bought tower for " + towerPrice + " gold. Remaining: " + currentGold);

                GameObject newTower = Instantiate(towerPrefab, cellCenter, Quaternion.identity);
                occupiedTiles[cell] = true;


                if (billboardManager != null)
                    billboardManager.RegisterSprite(newTower.transform);
            }
            else
            {
                Debug.Log("Not enough gold to place tower. You have: " + currentGold);
            }

            if (!Keyboard.current.shiftKey.isPressed || Keyboard.current.shiftKey.wasReleasedThisFrame)
                canPlaceTower = false;
        }

        // Sell tower
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                TowerPrice tower = hit.collider.GetComponent<TowerPrice>();
                if (tower != null)
                {
                    int refund = Mathf.RoundToInt(tower.price * towerSellMultiplier);
                    currentGold += refund;
                    Debug.Log("Sold tower for " + refund + " gold. Current gold: " + currentGold);

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
    // Remove any existing preview objects
    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Preview"))
        Destroy(obj);

    // Instantiate new preview tower
    previewTower = Instantiate(towerPrefab);
    previewTower.name = "PreviewTower";
    previewTower.tag = "Preview";

    // Disable root collider if it exists
    Collider col = previewTower.GetComponent<Collider>();
    if (col != null) col.enabled = false;

    // Disable all renderers and create unique materials
    previewRenderers = previewTower.GetComponentsInChildren<Renderer>();
    foreach (var rend in previewRenderers)
    {
        if (rend == null) continue;
        rend.material = new Material(rend.material);
        rend.enabled = false;
    }

    // Slightly scale up for preview effect
    previewTower.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);

    // Safely set isPreview on TowerTargeting (root or child)
    TowerTargeting targeting = previewTower.GetComponent<TowerTargeting>();
    if (targeting == null)
        targeting = previewTower.GetComponentInChildren<TowerTargeting>();

    if (targeting != null)
        targeting.isPreview = true;
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
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    Vector3Int smallCell = new(x, y, 0);
                    Vector3 cellCenter = tilemap.GetCellCenterWorld(smallCell);
                    if (Vector3.Distance(cellCenter, worldCenter) <= bigHexSize * 0.95f)
                        occupiedTiles[smallCell] = true;
                }
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
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    Vector3Int smallCell = new(x, y, 0);
                    Vector3 cellCenter = tilemap.GetCellCenterWorld(smallCell);
                    if (Vector3.Distance(cellCenter, worldCenter) <= bigHexSize * 1f)
                        isGroundTile[smallCell] = true;
                }
            }
        }
    }
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log("Gained " + amount + " gold. Current gold: " + currentGold);
    }
}
