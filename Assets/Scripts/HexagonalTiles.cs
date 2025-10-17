using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Tilemaps;
#endif
using System.Collections.Generic;

[ExecuteAlways]
public class HexagonalTiles : MonoBehaviour
{
    [Header("Main Settings")]
    public Tilemap sourceTilemap;
    public Tilemap targetTilemap;
    public GameObject prefabToSpawn;
    public Vector3 offset = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public bool clearOldObjects = true;

#if UNITY_EDITOR
    private GameObject parentContainer;
    private readonly Dictionary<Vector3Int, GameObject> spawnedObjects = new();
    private HashSet<Vector3Int> previousTiles = new();

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnSceneGUI(SceneView sceneView)
    {
        if (sourceTilemap == null || targetTilemap == null || prefabToSpawn == null) return;

        Event e = Event.current;
        if (e == null) return;

        // Only update on MouseUp to avoid lag
        if (e.type == EventType.MouseUp)
        {
            UpdateTiles();
        }
    }

    private void EnsureParent()
    {
        if (parentContainer == null)
        {
            parentContainer = GameObject.Find("SpawnedTileObjects") ?? new GameObject("SpawnedTileObjects");
            parentContainer.transform.SetParent(targetTilemap.transform, false);
        }
    }

    [ContextMenu("Sync Now")]
    public void UpdateTiles()
    {
        if (sourceTilemap == null || targetTilemap == null || prefabToSpawn == null) return;

        EnsureParent();

        // Get all active tiles in the source
        HashSet<Vector3Int> currentTiles = new();
        sourceTilemap.GetUsedTiles(currentTiles);

        // Clear objects that no longer exist
        if (clearOldObjects)
        {
            foreach (var pos in new List<Vector3Int>(spawnedObjects.Keys))
            {
                if (!currentTiles.Contains(pos))
                {
                    if (spawnedObjects[pos] != null)
                        DestroyImmediate(spawnedObjects[pos]);
                    spawnedObjects.Remove(pos);
                }
            }
        }

        // Spawn new objects
        foreach (var pos in currentTiles)
        {
            if (spawnedObjects.ContainsKey(pos)) continue;

            Vector3 worldPos = sourceTilemap.GetCellCenterWorld(pos);
            Vector3 targetWorld = targetTilemap.GetCellCenterWorld(targetTilemap.WorldToCell(worldPos));
            Vector3 finalPos = targetWorld + offset;

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            obj.transform.position = finalPos;
            obj.transform.rotation = Quaternion.Euler(rotation);
            obj.transform.SetParent(parentContainer.transform, true);

            spawnedObjects[pos] = obj;
        }

        EditorUtility.SetDirty(parentContainer);
        previousTiles = currentTiles;
    }
#endif
}

#if UNITY_EDITOR
public static class TilemapExtensions
{
    // Helper to get only active tile positions
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