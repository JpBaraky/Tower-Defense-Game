using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Places connected hex path tiles based on PathPiece prefabs.
/// Automatically aligns exits/entrances and updates path nodes.
/// </summary>
public class PathPlacer : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap groundTilemap;

    [Header("Path Prefabs")]
    public PathPiece startPiece;
    public PathPiece[] pathPrefabs;

    [Header("Placement Settings")]
    public int maxPieces = 10;
    public float hexRadius = 1f;

    private HashSet<Vector3Int> occupiedCells = new();
    private Vector3Int lastCell;
    private Transform lastExit;

    void Start()
    {
        if (startPiece == null || groundTilemap == null)
        {
            Debug.LogError("PathPlacer: Missing references!");
            return;
        }

        // Place starting piece
        lastCell = groundTilemap.WorldToCell(startPiece.transform.position);
        PlacePiece(startPiece, lastCell);

        if (startPiece.exits.Count > 0)
            lastExit = startPiece.exits[0];
    }

    [ContextMenu("Spawn Next Tile")]
    public void SpawnNextTile()
    {
        if (lastExit == null || pathPrefabs.Length == 0)
        {
            Debug.LogWarning("PathPlacer: No exit or prefabs defined.");
            return;
        }

        int attempts = 0;
        bool placed = false;

        while (!placed && attempts < 40)
        {
            attempts++;

            // Pick a random piece
            PathPiece prefab = pathPrefabs[Random.Range(0, pathPrefabs.Length)];
            PathPiece newPiece = Instantiate(prefab);

            bool fits = false;

            foreach (var newEntrance in newPiece.entrances)
            {
                // Get world direction from last exit
                Vector3 dir = lastExit.forward;
                Vector3 spawnPos = lastExit.position + dir * hexRadius * 1.5f;

                // Snap to cell center
                Vector3Int cell = groundTilemap.WorldToCell(spawnPos);
                if (occupiedCells.Contains(cell)) continue;

                // Place & rotate
                newPiece.transform.position = groundTilemap.GetCellCenterWorld(cell);

                // Rotate to align entrance with previous exit
                Vector3 toExit = (lastExit.position - newEntrance.position).normalized;
                float angle = Vector3.SignedAngle(newEntrance.forward, toExit, Vector3.up);
                newPiece.transform.Rotate(Vector3.up, angle, Space.World);

                // Mark as placed
                PlacePiece(newPiece, cell);
                lastCell = cell;

                // Set new exit
                lastExit = GetOtherExit(newPiece, newEntrance);
                fits = true;
                break;
            }

            if (fits)
                placed = true;
            else
                DestroyImmediate(newPiece.gameObject);
        }
    }

    private void PlacePiece(PathPiece piece, Vector3Int cell)
    {
        occupiedCells.Add(cell);
    }

    private Transform GetOtherExit(PathPiece piece, Transform usedEntrance)
    {
        foreach (var exit in piece.exits)
            if (exit != usedEntrance)
                return exit;
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var cell in occupiedCells)
        {
            Gizmos.DrawWireCube(groundTilemap.GetCellCenterWorld(cell), Vector3.one * 0.5f);
        }
    }
}
