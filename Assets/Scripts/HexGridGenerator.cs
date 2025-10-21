using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HexGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject hexPrefab;       // Prefab for each hex tile
    public int width = 10;             // Number of hexes horizontally
    public int height = 10;            // Number of hexes vertically
    public Transform parentObject;     // Parent for organization (optional)

    [Header("Hex Spacing")]
    public float hexWidth = 0.8659766f;   // Distance between hex centers (X)
    public float hexHeight = 0.8659766f;  // Distance between hex centers (Z)

    [Header("Rotation")]
    public float hexRotationY = 30f;   // Y rotation for each hex tile

    private Vector3 lastGridOrigin;    // Track position to detect movement

    private void OnEnable()
    {
        GenerateGrid();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Automatically rebuild in the Editor if moved or changed
        if (!Application.isPlaying)
        {
            if (transform.position != lastGridOrigin)
            {
                lastGridOrigin = transform.position;
                GenerateGrid();
            }
        }
    }
#endif

    [ContextMenu("Generate Hex Grid")]
    public void GenerateGrid()
    {
        if (hexPrefab == null)
        {
            Debug.LogWarning("Hex prefab not assigned.");
            return;
        }

        Transform parent = parentObject != null ? parentObject : transform;

        // Clear previous grid
#if UNITY_EDITOR
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            if (!Application.isPlaying)
                DestroyImmediate(parent.GetChild(i).gameObject);
            else
                Destroy(parent.GetChild(i).gameObject);
        }
#else
        foreach (Transform child in parent)
            Destroy(child.gameObject);
#endif

        // Spawn hexes
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Flat-top hex staggering: odd rows shifted by half width
                float xOffset = (y % 2 == 0) ? 0f : hexWidth / 2f;

                Vector3 position = new Vector3(
                    transform.position.x + x * hexWidth + xOffset,
                    transform.position.y,
                    transform.position.z + y * (hexHeight * 0.75f) // Vertical spacing for flat-top
                );

                Quaternion rotation = Quaternion.Euler(0f, hexRotationY, 0f);

                GameObject hex = (GameObject)Instantiate(hexPrefab, position, rotation, parent);
                hex.name = $"Hex_{x}_{y}";
            }
        }
    }
}
