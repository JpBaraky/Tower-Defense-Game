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
        var allNodes = FindObjectsByType<TileNode>(FindObjectsSortMode.None);
        if (allNodes.Length < 2) return;

        var start = allNodes.OrderBy(n => n.number).First();
        var end = allNodes.OrderBy(n => n.number).Last();

        transform.position = start.transform.position;

        if (useOptimalPath && pathfinder != null)
        {
            // A* path
            path = pathfinder.FindPath(start.transform.position, end.transform.position);
        }
        else
        {
            // Dumb path: strictly node number order
            path = allNodes.OrderBy(n => n.number).ToList();
        }
    }

    void Update()
    {
        if (path == null || currentIndex >= path.Count) return;

        Vector3 targetPos = path[currentIndex].transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            currentIndex++;
    }
}