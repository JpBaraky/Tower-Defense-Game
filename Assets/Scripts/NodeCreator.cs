using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class NodeCreator : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject objectPrefab;
    public Transform spawner;

    private List<TileNode> createdNodes = new List<TileNode>();
    private Dictionary<Vector3Int, TileBase> lastTiles = new Dictionary<Vector3Int, TileBase>();

    private readonly Vector3Int[] directions = new Vector3Int[]
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

    void Update()
    {
        if (TilesChanged())
        {
            RecreateNodes();
        }
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
        // Destroy old nodes
        foreach (TileNode node in createdNodes)
        {
            if (node != null)
                DestroyImmediate(node.gameObject);
        }
        createdNodes.Clear();

        // Create nodes from tiles
        Dictionary<Vector3Int, TileNode> nodeLookup = new Dictionary<Vector3Int, TileNode>();
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

        // Find castle
        GameObject castleObj = GameObject.FindGameObjectWithTag("Castle");
        if (castleObj == null)
        {
            Debug.LogWarning("No object with tag 'Castle' found.");
            return;
        }

        Vector3 castleWorldPos = castleObj.transform.position;

        // Find the node closest to the castle to be the last node
        TileNode lastNode = createdNodes
            .OrderBy(n => Vector3.Distance(n.transform.position, castleWorldPos))
            .First();

        // BFS to assign numbers and create coherent path
        Queue<TileNode> queue = new Queue<TileNode>();
        HashSet<TileNode> visited = new HashSet<TileNode>();

        lastNode.number = 0; // Start numbering from castle
        queue.Enqueue(lastNode);
        visited.Add(lastNode);

        while (queue.Count > 0)
        {
            TileNode current = queue.Dequeue();

            Vector3Int currentCell = tilemap.WorldToCell(current.transform.position);

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighborPos = currentCell + dir;

                if (nodeLookup.TryGetValue(neighborPos, out TileNode neighbor) && !visited.Contains(neighbor))
                {
                    neighbor.number = current.number + 1;
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        // Set spawner to the farthest node (max number)
        TileNode farthestNode = createdNodes.OrderByDescending(n => n.number).First();
        spawner.position = farthestNode.transform.position;
    }
}
