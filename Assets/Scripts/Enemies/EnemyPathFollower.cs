using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyFollowPath : MonoBehaviour
{
    public float speed = 2f;
    public bool useOptimalPath = true; // true = A*, false = node-by-node
    private Pathfinding pathfinder;
    private List<TileNode> path;
    private int currentIndex = 0;

    void Start()
    {
        pathfinder = FindAnyObjectByType<Pathfinding>();

        // Get all nodes
        var allNodes = FindObjectsByType<TileNode>(FindObjectsSortMode.None)
            .Where(n => n != null)
            .ToArray();

        if (allNodes == null || allNodes.Length < 2)
            return;

        var start = allNodes.OrderBy(n => n.number).FirstOrDefault();
        var end = allNodes.OrderBy(n => n.number).LastOrDefault();

        if (start == null || end == null)
            return;

        transform.position = start.transform.position;

        if (useOptimalPath && pathfinder != null)
        {
            // A* path
            path = pathfinder.FindPath(start.transform.position, end.transform.position);
        }
        else
        {
            // Dumb path: strictly node number order
            path = allNodes.Where(n => n != null).OrderBy(n => n.number).ToList();
        }
    }

    void Update()
    {
        if (path == null || path.Count == 0 || currentIndex >= path.Count)
            return;

        var currentNode = path[currentIndex];
        if (currentNode == null)
        {
            currentIndex++;
            return;
        }

        Vector3 targetPos = currentNode.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            currentIndex++;
    }
}