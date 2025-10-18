using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class RandomizeHexMaterial : MonoBehaviour
{
    [Range(0f, 1f)] public float randomizeOnStartChance = 1f;
    public Vector2 tilingRange = new Vector2(0.8f, 1.2f);
    public Vector2 offsetRange = new Vector2(0f, 1f);
    public bool randomizeRotation = true;

    private Material runtimeMat; // Unique material instance for this object

    void OnEnable()
    {
        ApplyRandomization();
    }

    void OnDisable()
    {
        // Clean up to avoid material leaks in Editor
        if (!Application.isPlaying && runtimeMat != null)
        {
            DestroyImmediate(runtimeMat);
            runtimeMat = null;
        }
    }

    void ApplyRandomization()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null || renderer.sharedMaterial == null)
            return;

        // Create a new instance of the shared material
        if (runtimeMat == null)
        {
            runtimeMat = new Material(renderer.sharedMaterial);
            renderer.sharedMaterial = runtimeMat;
        }

        // Random tiling
        float tilingX = Random.Range(tilingRange.x, tilingRange.y);
        float tilingY = tilingX; // Uniform scaling for hex tiles
        runtimeMat.mainTextureScale = new Vector2(tilingX, tilingY);

        // Random offset
        float offsetX = Random.Range(offsetRange.x, offsetRange.y);
        float offsetY = Random.Range(offsetRange.x, offsetRange.y);
        runtimeMat.mainTextureOffset = new Vector2(offsetX, offsetY);

        // Random rotation (if shader supports it)
        if (randomizeRotation && runtimeMat.HasProperty("_Rotation"))
        {
            runtimeMat.SetFloat("_Rotation", Random.Range(0f, 360f));
        }
    }
}
