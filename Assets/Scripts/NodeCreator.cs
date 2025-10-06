using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class NodeCreator : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject objectPrefab;
    public Transform spawner; // Enemy spawner

    void Start()
    {
        List<TileNode> createdNodes = new List<TileNode>();

        // 1. Create all nodes
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
                        createdNodes.Add(node);
                }
            }
        }

        // 2. Find the node closest to the spawner
        TileNode current = createdNodes.OrderBy(n => Vector3.Distance(n.transform.position, spawner.position)).First();
        current.number = 0;

        // 3. Number remaining nodes based on nearest neighbor
        int counter = 1;
        List<TileNode> unnumbered = createdNodes.Where(n => n != current).ToList();

        while (unnumbered.Count > 0)
        {
            // Find nearest unnumbered neighbor
            TileNode next = unnumbered.OrderBy(n => Vector3.Distance(n.transform.position, current.transform.position)).First();
            next.number = counter++;
            current = next;
            unnumbered.Remove(next);
        }
    }
}