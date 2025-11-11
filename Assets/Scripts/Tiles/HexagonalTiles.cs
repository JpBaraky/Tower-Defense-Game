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
    private bool needsUpdate = false;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update += OnEditorUpdate;
        Tilemap.tilemapTileChanged += OnTilemapChanged;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= OnEditorUpdate;
        Tilemap.tilemapTileChanged -= OnTilemapChanged;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e != null && e.type == EventType.MouseUp)
        {
            UpdateTilesFull();
        }
    }

    private void OnEditorUpdate()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
            UpdateTilesFull();
        }
    }

    private void OnTilemapChanged(Tilemap tilemap, Tilemap.SyncTile[] changes)
    {
        if (tilemap != sourceTilemap) return;
        needsUpdate = true;
    }

    private void Start()
    {
        EnsureParent();
        RebuildSpawnedObjects(); // Prevent duplicates at start
        UpdateTilesFull();
    }

    private void EnsureParent()
    {
        if (parentContainer == null)
        {
            parentContainer = this.transform.gameObject;
            parentContainer.transform.SetParent(targetTilemap.transform, false);
        }
    }

    [ContextMenu("Sync Now")]
    public void UpdateTilesFull()
    {
        if (sourceTilemap == null || targetTilemap == null || prefabToSpawn == null)
            return;

        EnsureParent();

        // Get all active tiles
        HashSet<Vector3Int> currentTiles = new();
        sourceTilemap.GetUsedTiles(currentTiles);

        // Remove objects for tiles no longer present
        if (clearOldObjects)
        {
            foreach (var pos in new List<Vector3Int>(spawnedObjects.Keys))
            {
                if (!currentTiles.Contains(pos))
                    RemoveTile(pos);
            }
        }

        // Spawn or keep tiles
        foreach (var pos in currentTiles)
        {
            SpawnOrKeep(pos);
        }

        EditorUtility.SetDirty(parentContainer);
    }

    private void SpawnOrKeep(Vector3Int pos)
    {
        GameObject existing = null;
        spawnedObjects.TryGetValue(pos, out existing);

        // If a Path object is already at this cell, reuse it
        if (existing != null && existing.CompareTag("Path"))
        {
            spawnedObjects[pos] = existing;
            return;
        }

        // If a non-Path object is already there, reuse it
        if (existing != null)
        {
            spawnedObjects[pos] = existing;
            return;
        }

        // Check for any Path object in children at this cell
        GameObject pathObj = FindPathObjectAtCell(pos);
        if (pathObj != null)
        {
            spawnedObjects[pos] = pathObj;
            return;
        }

        // Spawn new prefab
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
        if (obj == null) return;

        Vector3 worldPos = targetTilemap.GetCellCenterWorld(pos) + offset;
        obj.transform.position = worldPos;
        obj.transform.rotation = Quaternion.Euler(rotation);
        obj.transform.SetParent(parentContainer.transform, true);

        var info = obj.GetComponent<SpawnedTileInfo>() ?? obj.AddComponent<SpawnedTileInfo>();
        info.sourceTilemap = sourceTilemap;

        spawnedObjects[pos] = obj;
    }

    private void RemoveTile(Vector3Int pos)
    {
        if (!spawnedObjects.TryGetValue(pos, out GameObject obj)) return;

        spawnedObjects.Remove(pos);

        if (obj != null)
        {
            var info = obj.GetComponent<SpawnedTileInfo>();
            if (info != null && info.sourceTilemap == sourceTilemap)
            {
                DestroyImmediate(obj);
            }
        }
    }

    private void RebuildSpawnedObjects()
    {
        spawnedObjects.Clear();

        foreach (Transform child in parentContainer.transform)
        {
            if (child == null) continue;

            Vector3Int cellPos = targetTilemap.WorldToCell(child.position);
            var info = child.GetComponent<SpawnedTileInfo>();

            if (info != null && info.sourceTilemap == sourceTilemap)
            {
                if (!spawnedObjects.ContainsKey(cellPos))
                    spawnedObjects[cellPos] = child.gameObject;
                else
                {
                    // Prefer Path object over non-Path
                    if (!spawnedObjects[cellPos].CompareTag("Path") && child.CompareTag("Path"))
                        spawnedObjects[cellPos] = child.gameObject;
                    else
                        DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private GameObject FindPathObjectAtCell(Vector3Int cellPos)
    {
        foreach (Transform child in parentContainer.transform)
        {
            if (child == null) continue;

            Vector3Int childCell = targetTilemap.WorldToCell(child.position);
            if (childCell == cellPos && child.CompareTag("Path"))
                return child.gameObject;
        }
        return null;
    }
#endif
}


