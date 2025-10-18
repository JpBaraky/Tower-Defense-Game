using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    [Header("Hex Settings")]
    public GameObject smallHexPrefab;
    [Range(0.01f, 1f)]
    public float hexScale = 0.2f; // Proportion relative to this hex

    [ContextMenu("Generate Grid")]
    void GenerateGrid()
    {
        if (smallHexPrefab == null)
        {
            Debug.LogError("Assign the smallHexPrefab!");
            return;
        }

        // Clean up old hexes
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        float parentRadius = 1f; // Assume this GameObject has a size of 1 unit radius
        float smallRadius = parentRadius * hexScale;

        float xOffset = smallRadius * 1.5f;
        float yOffset = smallRadius * Mathf.Sqrt(3);

        // Estimate how many rings fit
        int maxRing = Mathf.FloorToInt(1f / hexScale);

        for (int q = -maxRing; q <= maxRing; q++)
        {
            int r1 = Mathf.Max(-maxRing, -q - maxRing);
            int r2 = Mathf.Min(maxRing, -q + maxRing);
            for (int r = r1; r <= r2; r++)
            {
                Vector2 pos = HexToWorld_Pointy(q, r, smallRadius);
                if (IsInsideHexagon(pos, parentRadius))
                {
                    GameObject hex = Instantiate(smallHexPrefab, transform);
                    hex.transform.localPosition = new Vector3(pos.x, 0, pos.y); // Y-up world
                    hex.transform.localScale = Vector3.one * hexScale;
                }
            }
        }
    }

    // Converts axial coordinates (q, r) to world position for pointy-topped hexes
    Vector2 HexToWorld_Pointy(int q, int r, float radius)
    {
        float x = radius * Mathf.Sqrt(3f) * (q + r / 2f);
        float y = radius * 1.5f * r;
        return new Vector2(x, y);
    }

    // Checks whether a 2D point is inside the large pointy-top hexagon (radius = 1)
    bool IsInsideHexagon(Vector2 point, float radius)
    {
        // Convert back to axial coordinates and check hex distance
        float q = (Mathf.Sqrt(3f)/3f * point.x - 1f/3f * point.y) / radius;
        float r = (2f/3f * point.y) / radius;
        float s = -q - r;

        return Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(s)) <= 1f;
    }
}
