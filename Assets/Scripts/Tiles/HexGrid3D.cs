using UnityEngine;

public class HexGrid3D : MonoBehaviour
{
    [Header("Existing 2D Grid")]
    public GameObject grid2DObject; // Assign your 2D grid GameObject here

    [Header("Hex Prefab")]
    public GameObject hexPrefab;     // 3D hex prism prefab
    public Transform hexParent;      // Optional parent for hierarchy

    [Header("Height Settings")]
    public float hexHeightOffset = 0f; // Raise the 3D hex above the 2D tiles if needed

    [Header("Spawn Settings")]
    public float overlapCheckRadius = 0.1f; // Small radius to detect existing hex

    void Start()
    {
        if (grid2DObject == null)
        {
            Debug.LogError("Please assign the 2D grid GameObject!");
            return;
        }

        if (hexPrefab == null)
        {
            Debug.LogError("Please assign a 3D hex prefab!");
            return;
        }

        SpawnHexesFromGrid();
    }

    void SpawnHexesFromGrid()
    {
        foreach (Transform tile in grid2DObject.transform)
        {
            Vector3 worldPos = tile.position;
            worldPos.y += hexHeightOffset;

            // Check if a hex already exists at this position
            if (!HexExistsAtPosition(worldPos))
            {
                GameObject hex3D = Instantiate(hexPrefab, worldPos, Quaternion.identity);

                if (hexParent != null)
                    hex3D.transform.parent = hexParent;

                hex3D.name = $"Hex3D_{tile.name}";
            }
        }
    }

    bool HexExistsAtPosition(Vector3 position)
    {
        // Use a small sphere overlap to detect existing hexes
        Collider[] colliders = Physics.OverlapSphere(position, overlapCheckRadius);
        foreach (Collider col in colliders)
        {
            if (col.gameObject.name.StartsWith("Hex3D_")) // Optional: only check objects spawned by this script
                return true;
        }
        return false;
    }
}
