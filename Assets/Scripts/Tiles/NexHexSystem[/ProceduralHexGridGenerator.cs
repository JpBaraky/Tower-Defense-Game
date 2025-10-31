using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
#endif

public class HexSubTileGenerator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject smallHexPrefab;
    public float smallHexRadius = 0.33f; // 1/3 of main hex
    public float heightStep = 0.2f;
    public int steps = 4; // 0, 0.2, 0.4, 0.6

    private List<SmallHex> smallHexes = new List<SmallHex>();

    void Start()
    {
        GenerateSmallHexes();
        AssignHeights();
    }

  void GenerateSmallHexes()
{
    smallHexes.Clear();

    // Center hex
    var centerHex = Instantiate(smallHexPrefab, transform.position, Quaternion.Euler(0f, 30f, 0f), transform);
    centerHex.transform.localScale = Vector3.one * smallHexRadius;
    smallHexes.Add(new SmallHex(centerHex));

    // Distance between centers of touching hexes for pointy-top layout
    float offset = smallHexRadius * 0.866f;

        // Generate 6 surrounding hexes
        for (int i = 0; i < 6; i++)
        {
            float angleDeg = 60f * i;
            float angleRad = Mathf.Deg2Rad * angleDeg;

            // Compute direction for each surrounding hex
            Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad)) * offset;

            // Calculate position
            Vector3 pos = transform.position + dir;

            var hexObj = Instantiate(smallHexPrefab, pos, Quaternion.Euler(0f, 30f, 0f), transform);
            hexObj.transform.localScale = Vector3.one * smallHexRadius;

            smallHexes.Add(new SmallHex(hexObj));
        }
    this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
}



    void AssignHeights()
    {
        foreach (var hex in smallHexes)
        {
            
            hex.height = -1f;
            
        }

        // Start with the center random height
        float[] possibleHeights = new float[steps];
        for (int i = 0; i < steps; i++) possibleHeights[i] = i * heightStep;

        smallHexes[0].height = possibleHeights[Random.Range(0, possibleHeights.Length)];

        // Assign neighbors respecting the rule
        for (int i = 1; i < smallHexes.Count; i++)
        {
            float baseHeight = smallHexes[0].height;
            float minAllowed = Mathf.Max(0, baseHeight - heightStep);
            float maxAllowed = Mathf.Min((steps - 1) * heightStep, baseHeight + heightStep);

            // pick a value that’s a multiple of heightStep within range
            List<float> allowed = new List<float>();
            foreach (float h in possibleHeights)
                if (h >= minAllowed && h <= maxAllowed)
                    allowed.Add(h);

            smallHexes[i].height = allowed[Random.Range(0, allowed.Count)];
        }

        // Apply to the mesh or probuilder shape
        foreach (var hex in smallHexes)
        {
var pb = hex.obj.GetComponent<ProBuilderMesh>();
if (pb != null)
{
    Vector3 scale = hex.obj.transform.localScale;
    scale.y = hex.height + 1f;
    hex.obj.transform.localScale = scale;
    pb.ToMesh();
    pb.Refresh();
}
            {
                // fallback for normal mesh
                Vector3 s = hex.obj.transform.localScale;
                s.y = hex.height + 0.051f; // small offset to avoid z-fighting
                hex.obj.transform.localScale = s;
            }
        }
    }

    class SmallHex
    {
        public GameObject obj;
        public float height;
        public SmallHex(GameObject o) { obj = o; }
    }
}
