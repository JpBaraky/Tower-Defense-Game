using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PixelPerfectHexBrush", menuName = "Brushes/PixelPerfectHexBrush")]
public class PixelPerfectHexBrush : ScriptableObject
{
#if UNITY_EDITOR
    public float hexSize = 1f; // radius of hex in world units
    public float pixelsPerUnit = 16f; // match your sprite PPU

    /// <summary>
    /// Paint a tile at a world position snapping to point-top hex grid
    /// </summary>
    public void Paint(Tilemap tilemap, Vector3 worldPos, TileBase tile)
    {
        if (tile == null) return;

        // Convert world pos to hex axial coordinates
        float q = (Mathf.Sqrt(3f)/3f * worldPos.x - 1f/3f * worldPos.y) / hexSize;
        float r = (2f/3f * worldPos.y) / hexSize;

        // Round to nearest hex
        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);

        // Convert back to world position
        float x = hexSize * Mathf.Sqrt(3f) * (rq + rr/2f);
        float y = hexSize * 3f/2f * rr;

        // Pixel-perfect snap
        x = Mathf.Round(x * pixelsPerUnit) / pixelsPerUnit;
        y = Mathf.Round(y * pixelsPerUnit) / pixelsPerUnit;

        Vector3Int cellPos = tilemap.WorldToCell(new Vector3(x, y, 0));
        tilemap.SetTile(cellPos, tile);
        tilemap.RefreshTile(cellPos);
    }

    /// <summary>
    /// Erase a tile at a world position snapping to hex grid
    /// </summary>
    public void Erase(Tilemap tilemap, Vector3 worldPos)
    {
        // Same snapping logic
        float q = (Mathf.Sqrt(3f)/3f * worldPos.x - 1f/3f * worldPos.y) / hexSize;
        float r = (2f/3f * worldPos.y) / hexSize;

        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);

        float x = hexSize * Mathf.Sqrt(3f) * (rq + rr/2f);
        float y = hexSize * 3f/2f * rr;

        x = Mathf.Round(x * pixelsPerUnit) / pixelsPerUnit;
        y = Mathf.Round(y * pixelsPerUnit) / pixelsPerUnit;

        Vector3Int cellPos = tilemap.WorldToCell(new Vector3(x, y, 0));
        tilemap.SetTile(cellPos, null);
        tilemap.RefreshTile(cellPos);
    }
#endif
}
