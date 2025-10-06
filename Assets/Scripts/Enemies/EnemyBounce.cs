using UnityEngine;

public class EnemyBounce : MonoBehaviour
{ public float swayAmount = 0.1f;   // side-to-side distance
    public float swaySpeed = 6f;      // how fast it sways
    public float bounceHeight = 0.05f; // small up-down motion
    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float yOffset = Mathf.Abs(Mathf.Cos(Time.time * swaySpeed)) * bounceHeight; // subtle up-down
        transform.localPosition = startLocalPos + new Vector3(xOffset, yOffset, 0);
    }
}