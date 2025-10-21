using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class RandomizeHexMaterial : MonoBehaviour
{
    [Header("Randomization Settings")]
    [Range(0f, 1f)] public float randomizeOnStartChance = 1f;
    public Vector2 tilingRange = new Vector2(0.8f, 1.2f);
    public Vector2 offsetRange = new Vector2(0f, 1f);
    public bool randomizeRotation = true;

    private Material previewMat;   // Editor preview material (not saved)
    private Material runtimeMat;   // Runtime instance (for play mode only)
    private Renderer rend;

    void OnEnable()
    {
        rend = GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null)
            return;

        if (Application.isPlaying)
            ApplyRuntimeRandomization();
        else
            ApplyEditorPreview();
    }

    void OnDisable()
    {
        // Clean up only runtime material (not prefab safe)
        if (Application.isPlaying && runtimeMat != null)
        {
            Destroy(runtimeMat);
            runtimeMat = null;
        }
    }

    // ---------- RUNTIME ----------
    void ApplyRuntimeRandomization()
    {
        // Unity automatically creates a unique material when using .material
        runtimeMat = rend.material;
        RandomizeMaterial(runtimeMat);
    }

    // ---------- EDITOR ----------
    void ApplyEditorPreview()
    {
        // Avoid creating infinite preview copies
        if (previewMat == null)
        {
            previewMat = new Material(rend.sharedMaterial)
            {
                name = rend.sharedMaterial.name + " (Preview)"
            };
        }

        rend.sharedMaterial = previewMat;
        RandomizeMaterial(previewMat);
    }

    // ---------- COMMON LOGIC ----------
    void RandomizeMaterial(Material mat)
    {
        if (Random.value > randomizeOnStartChance || mat == null)
            return;

        // Random tiling
        float tilingX = Random.Range(tilingRange.x, tilingRange.y);
        float tilingY = tilingX;
        mat.mainTextureScale = new Vector2(tilingX, tilingY);

        // Random offset
        float offsetX = Random.Range(offsetRange.x, offsetRange.y);
        float offsetY = Random.Range(offsetRange.x, offsetRange.y);
        mat.mainTextureOffset = new Vector2(offsetX, offsetY);

        // Random rotation if supported
        if (randomizeRotation && mat.HasProperty("_Rotation"))
        {
            mat.SetFloat("_Rotation", Random.Range(0f, 360f));
        }
    }
}