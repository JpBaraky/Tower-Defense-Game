using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class SpawnedTileInfo : MonoBehaviour
{
    public Tilemap sourceTilemap;
}

#if UNITY_EDITOR
public static class TilemapExtensions
{
    public static void GetUsedTiles(this Tilemap tilemap, HashSet<Vector3Int> positions)
    {
        positions.Clear();
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
                positions.Add(pos);
        }
    }
}
#endif