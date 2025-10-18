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
        // Loop through all child tiles of the 2D grid
        foreach (Transform tile in grid2DObject.transform)
        {
            Vector3 worldPos = tile.position;
            worldPos.y += hexHeightOffset; // optional vertical offset

            GameObject hex3D = Instantiate(hexPrefab, worldPos, Quaternion.identity);

            if (hexParent != null)
                hex3D.transform.parent = hexParent;

            hex3D.name = $"Hex3D_{tile.name}";
        }
    }
}