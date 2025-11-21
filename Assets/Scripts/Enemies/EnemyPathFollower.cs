using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyFollowPath : MonoBehaviour
{
    [Header("Settings")]
    public bool useOptimalPath = true;

    private Pathfinding pathfinder;
    private List<TileNode> path;
    [HideInInspector]
    public int currentIndex = 0;

    public Transform nextNode;

    private bool spawnPositionOverridden = false;

    void Start()
    {
        StartCoroutine(InitAfterNodesExist());
    }

    IEnumerator InitAfterNodesExist()
    {
        // wait one frame so tile graph exists
        yield return null;

        pathfinder = FindAnyObjectByType<Pathfinding>();

        TileNode[] allNodes = GameObject.FindGameObjectsWithTag("Node")
            .Select(go => go.GetComponent<TileNode>())
            .Where(n => n != null)
            .ToArray();

        if (allNodes == null || allNodes.Length < 2)
            yield break;

        TileNode start = allNodes.OrderBy(n => n.number).First();
        TileNode end = allNodes.OrderBy(n => n.number).Last();

        // ONLY move to start if nothing else manually positioned it
        if (!spawnPositionOverridden)
            transform.position = start.transform.position;

        // Build final path
        if (useOptimalPath && pathfinder != null)
            path = pathfinder.FindPath(start.transform.position, end.transform.position);
        else
            path = allNodes.OrderBy(n => n.number).ToList();
    }

    // Called externally when spawning enemies at a manual position
    public void SetSpawnPosition(Vector3 pos)
    {
        spawnPositionOverridden = true;
        transform.position = pos;
    }

    // Forces the enemy to continue from the parent’s node, not from node 0
    public void SetCurrentNode(TileNode node)
    {
        if (path == null || path.Count == 0)
            return;

        int index = path.IndexOf(node);

        // If node not found, fallback to nearest
        if (index == -1)
        {
            index = path
                .Select((n, i) => new { i, d = Vector3.Distance(n.transform.position, node.transform.position) })
                .OrderBy(x => x.d)
                .First().i;
        }

        currentIndex = index;
        transform.position = path[currentIndex].transform.position;
    }

    void Update()
    {
        if (path == null || path.Count == 0 || currentIndex >= path.Count)
            return;

        TileNode node = path[currentIndex];
        if (node == null)
        {
            currentIndex++;
            return;
        }

        nextNode = node.transform;

        float moveSpeed = GetComponent<Enemy>().moveSpeed;
        Vector3 targetPos = node.transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            currentIndex++;
    }
}

