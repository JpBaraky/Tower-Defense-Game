using UnityEngine;

public class EnemyBounce : MonoBehaviour
{
    public float swayAmount = 0.1f;    // side-to-side distance
    public float swaySpeed = 6f;       // how fast it sways
    public float bounceHeight = 0.05f; // small up-down motion

    private Vector3 basePosition;

    void LateUpdate()
    {
        // Capture the current position from path-following object
        basePosition = transform.position;

        // Sway side-to-side
        float xOffset = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        // Bounce up and down
        float yOffset = Mathf.Abs(Mathf.Cos(Time.time * swaySpeed)) * bounceHeight;

        Vector3 offset = new Vector3(xOffset, yOffset, 0);

        // Apply offset on top of current moving position
        transform.position = basePosition + offset;
    }
}
