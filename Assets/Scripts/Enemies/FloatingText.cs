using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float fadeDuration = 1f;

    private TextMeshProUGUI textMesh;
    private Color startColor;
    private Transform cam;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        cam = Camera.main.transform; // cache camera
        startColor = textMesh.color;
    }

    public void Initialize(string text)
    {
        textMesh.text = text;
        
    }

    void Update()
    {
        // Always face camera
        if (cam != null)
            transform.forward = cam.forward;

        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        startColor.a -= Time.deltaTime / fadeDuration;
        textMesh.color = startColor;

        if (startColor.a <= 0f)
            Destroy(gameObject);
    }
}
