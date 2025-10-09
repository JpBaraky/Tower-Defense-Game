using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class NodeCreator : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject objectPrefab;
    public Transform spawner;

    private List<TileNode> createdNodes = new List<TileNode>();
    private Dictionary<Vector3Int, TileNode> nodeLookup = new Dictionary<Vector3Int, TileNode>();
    private Dictionary<Vector3Int, TileBase> lastTiles = new Dictionary<Vector3Int, TileBase>();
    private List<TileNode> mainPath = new List<TileNode>();
    private float neighborThreshold = 1.1f; // slightly larger than cell size to account for floating errors

   
    void Update()
    {
        if (TilesChanged())
            RecreateNodes();
    }

    bool TilesChanged()
    {
        bool changed = false;
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase currentTile = tilemap.GetTile(pos);
                if (!lastTiles.TryGetValue(pos, out TileBase lastTile) || lastTile != currentTile)
                {
                    changed = true;
                    lastTiles[pos] = currentTile;
                }
            }
        }
        return changed;
    }
  

    void RecreateNodes()
    {
        // -----------------------
        // 1. Destroy old nodes
        // -----------------------
        
         GameObject[] nodesToDelete = GameObject.FindGameObjectsWithTag("Node");

foreach (GameObject node in nodesToDelete)
{
    if (node != null)
    {
        if (Application.isPlaying)
            Destroy(node);
        else
            DestroyImmediate(node);
    }
}

// Clear tracking lists
createdNodes.Clear();
nodeLookup.Clear();
mainPath.Clear();

        // -----------------------
        // 2. Create nodes for all tiles
        // -----------------------
        for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
        {
            for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null)
                {
                    Vector3 worldPos = tilemap.GetCellCenterWorld(pos);
                    GameObject obj = Instantiate(objectPrefab, worldPos, Quaternion.identity);
                    obj.transform.SetParent(tilemap.transform, true);
                    TileNode node = obj.GetComponent<TileNode>();
                    if (node != null)
                    {
                        createdNodes.Add(node);
                        nodeLookup[pos] = node;
                    }
                }
            }
        }

        if (createdNodes.Count == 0) return;

        // -----------------------
        // 3. Castle node
        // -----------------------
        GameObject castleObj = GameObject.FindGameObjectWithTag("Castle");
        if (castleObj == null)
        {
            Debug.LogWarning("No object with tag 'Castle' found.");
            return;
        }

        Vector3Int castleCell = tilemap.WorldToCell(castleObj.transform.position);
        TileNode castleNode;
        if (!nodeLookup.TryGetValue(castleCell, out castleNode))
        {
            GameObject obj = Instantiate(objectPrefab, tilemap.GetCellCenterWorld(castleCell), Quaternion.identity);
            obj.transform.SetParent(tilemap.transform, true);
            castleNode = obj.GetComponent<TileNode>();
            createdNodes.Add(castleNode);
            nodeLookup[castleCell] = castleNode;
        }

        // -----------------------
        // 4. BFS distances from castle
        // -----------------------
        Dictionary<TileNode, TileNode> parents = new Dictionary<TileNode, TileNode>();
        Dictionary<TileNode, int> distances = new Dictionary<TileNode, int>();
        Queue<TileNode> queue = new Queue<TileNode>();
        queue.Enqueue(castleNode);
        distances[castleNode] = 0;

        while (queue.Count > 0)
        {
            TileNode current = queue.Dequeue();

            foreach (TileNode neighbor in GetNeighbors(current))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[current] + 1;
                    parents[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // -----------------------
        // 5. Spawner node = farthest from castle
        // -----------------------
        TileNode spawnerNode = distances.OrderByDescending(kv => kv.Value).First().Key;
        spawnerNode.number = 0;
        spawner.position = GetSpawnerEdgePosition(spawnerNode, parents.ContainsKey(spawnerNode) ? parents[spawnerNode] : null);

        // -----------------------
        // 6. Build full path visiting all nodes
        // -----------------------
        mainPath.Clear();
        HashSet<TileNode> visited = new HashSet<TileNode>();
        Queue<TileNode> toVisit = new Queue<TileNode>();
        toVisit.Enqueue(spawnerNode);
        visited.Add(spawnerNode);
        int counter = 0;

        while (toVisit.Count > 0)
        {
            TileNode current = toVisit.Dequeue();
            current.number = counter++;
            mainPath.Add(current);

            foreach (TileNode neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        // Castle = last node
        castleNode.number = counter - 1;
        if (!mainPath.Contains(castleNode))
            mainPath.Add(castleNode);
    }

    List<TileNode> GetNeighbors(TileNode node)
    {
        List<TileNode> neighbors = new List<TileNode>();
        foreach (TileNode other in createdNodes)
        {
            if (other == node) continue;
            if (Vector3.Distance(node.transform.position, other.transform.position) <= tilemap.cellSize.x * neighborThreshold)
                neighbors.Add(other);
        }
        return neighbors;
    }

    private Vector3 GetSpawnerEdgePosition(TileNode spawnerNode, TileNode nextNode)
    {
        if (nextNode == null) return spawnerNode.transform.position;
        Vector3 dir = (spawnerNode.transform.position - nextNode.transform.position).normalized;
        float halfTile = tilemap.cellSize.x * 0.5f;
        return spawnerNode.transform.position + dir * halfTile;
    }
 void OnDrawGizmos()
    {
        if (mainPath == null || mainPath.Count == 0) return;

        for (int i = 0; i < mainPath.Count; i++)
        {
            TileNode node = mainPath[i];
            if (node == null) continue;

            Gizmos.color = Color.Lerp(Color.blue, Color.red, i / (float)(mainPath.Count - 1));
            Gizmos.DrawSphere(node.transform.position, 0.1f);

#if UNITY_EDITOR
            Handles.Label(node.transform.position + Vector3.up * 0.2f, node.number.ToString());
#endif
        }
    }
}
