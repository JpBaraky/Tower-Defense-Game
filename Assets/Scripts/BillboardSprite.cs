using System.Collections.Generic;
using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Starting Sprites")]
    public List<Transform> startingSprites = new List<Transform>();

    private List<Transform> sprites = new List<Transform>();

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Add starting sprites
        sprites.AddRange(startingSprites);
    }

    void LateUpdate()
    {
        Vector3 camPos = mainCamera.transform.position;

        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i] == null) continue;

            Vector3 dir = camPos - sprites[i].position;
            dir.y = 0; // keep upright

            if (dir.sqrMagnitude > 0.001f)
                sprites[i].rotation = Quaternion.LookRotation(dir);
        }
    }

    /// <summary>Register a new sprite at runtime</summary>
    public void RegisterSprite(Transform sprite)
    {
        if (!sprites.Contains(sprite))
            sprites.Add(sprite);
    }

    /// <summary>Optional: remove a sprite if destroyed</summary>
    public void UnregisterSprite(Transform sprite)
    {
        if (sprites.Contains(sprite))
            sprites.Remove(sprite);
    }
}