using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyFollowPath : MonoBehaviour
{
    
    public bool useOptimalPath = true;
    private Pathfinding pathfinder;
    private List<TileNode> path;
    private int currentIndex = 0;
    public Transform nextNode;
    private Enemy enemy;

    void Start()
    {
        StartCoroutine(InitAfterNodesExist());
        enemy = GetComponent<Enemy>();
        
    }

    IEnumerator InitAfterNodesExist()
    {
        // wait one frame so NodeCreator can finish
        yield return null; 

        pathfinder = FindAnyObjectByType<Pathfinding>();
        var allNodes = GameObject.FindGameObjectsWithTag("Node")
            .Select(go => go.GetComponent<TileNode>())
            .Where(n => n != null)
            .ToArray();

        if (allNodes == null || allNodes.Length < 2)
            yield break;

        var start = allNodes.OrderBy(n => n.number).FirstOrDefault();
        var end = allNodes.OrderBy(n => n.number).LastOrDefault();

        if (start == null || end == null)
            yield break;

        // start enemy at spawn node
        transform.position = start.transform.position;

        if (useOptimalPath && pathfinder != null)
            path = pathfinder.FindPath(start.transform.position, end.transform.position);
        else
            path = allNodes.OrderBy(n => n.number).ToList();
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
        nextNode = currentNode.transform;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, enemy.moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            currentIndex++;
    }
}