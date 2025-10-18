using UnityEngine;
using System.Collections.Generic;

public class PathGenerator : MonoBehaviour
{
    [Header("Setup")]
    public PathPiece startPiece;        // First placed tile
    public PathPiece[] pathPrefabs;     // Tiles to spawn

    private float hexRadius;
    private HashSet<Vector2Int> occupiedHexes = new();
    private Vector2Int lastCoord;
    private Transform lastExit;

    void Start()
    {
        // Place first tile at origin
        lastCoord = Vector2Int.zero;
        PlacePiece(startPiece, lastCoord);

        // Pick first exit to grow path
        if (startPiece.exits.Count > 0)
            lastExit = startPiece.exits[0];

        // Compute hex radius from your chosen offset
        Vector3 firstWorld = startPiece.transform.position;
        Vector3 secondWorld = new Vector3(-0.25f, 0, -0.75f); // bottom-left example
        float dz = Mathf.Abs(secondWorld.z - firstWorld.z);
        hexRadius = dz / 1.5f;
    }

    public void SpawnNextTile()
    {
        if (lastExit == null || pathPrefabs.Length == 0) return;

        int attempts = 0;
        bool placed = false;

        while (!placed && attempts < 50)
        {
            attempts++;

            // Pick random prefab
            PathPiece prefab = pathPrefabs[Random.Range(0, pathPrefabs.Length)];
            PathPiece newPiece = Instantiate(prefab);
            newPiece.Awake();

            bool fits = false;

            // Try all exits of the new piece
            foreach (var newExit in newPiece.exits)
            {
                Vector2Int dir = HexDirectionToAxial(newExit);
                Vector2Int targetCoord = lastCoord + dir;

                if (occupiedHexes.Contains(targetCoord)) continue;

                // Snap to grid
                newPiece.transform.position = AxialToWorld(targetCoord, hexRadius);

                // Rotate to match last exit
                Vector3 toFromExit = (lastExit.position - newExit.position).normalized;
                float angle = Vector3.SignedAngle(newExit.forward, toFromExit, Vector3.up);
                newPiece.transform.Rotate(0, angle, 0, Space.World);

                // Place piece
                PlacePiece(newPiece, targetCoord);

                // Update last tile info
                lastCoord = targetCoord;
                lastExit = GetOtherExit(newPiece, newExit);
                fits = true;
                break;
            }

            if (fits)
                placed = true;
            else
                Destroy(newPiece.gameObject);
        }
    }

    void PlacePiece(PathPiece piece, Vector2Int coord)
    {
        occupiedHexes.Add(coord);
    }

    Transform GetOtherExit(PathPiece piece, Transform usedExit)
    {
        foreach (var exit in piece.exits)
            if (exit != usedExit)
                return exit;
        return null;
    }

    Vector3 AxialToWorld(Vector2Int hex, float radius)
    {
        float x = radius * Mathf.Sqrt(3) * hex.x + (radius * Mathf.Sqrt(3) / 2f) * hex.y;
        float z = radius * 1.5f * hex.y;
        return new Vector3(x, 0, z);
    }

    Vector2Int HexDirectionToAxial(Transform exit)
    {
        Vector3 localDir = exit.forward;
        float angle = Mathf.Atan2(localDir.z, localDir.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f;

        if (angle < 30 || angle >= 330) return new Vector2Int(1, 0);
        if (angle < 90) return new Vector2Int(1, -1);
        if (angle < 150) return new Vector2Int(0, -1);
        if (angle < 210) return new Vector2Int(-1, 0);
        if (angle < 270) return new Vector2Int(-1, 1);
        return new Vector2Int(0, 1);
    }
}
