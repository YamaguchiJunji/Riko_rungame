using UnityEngine;

public class camera_follow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;

    [Header("Vertical Follow")]
    [Range(0.5f, 1f)]
    public float verticalFollowThreshold = 0.85f;

    private float velocityX = 0f;
    private float velocityY = 0f;
    private float defaultY;
    private Camera cam;

    public GameObject background;

    private float backgroundDefaultY;
    private float backgroundDefaultZ;

    void Start()
    {
        cam = GetComponent<Camera>();
        defaultY = transform.position.y;

        if (background != null)
        {
            backgroundDefaultY = background.transform.position.y;
            backgroundDefaultZ = background.transform.position.z;
        }
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        float newX = Mathf.SmoothDamp(transform.position.x, target.position.x, ref velocityX, smoothTime);
        float targetY = defaultY;

        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);

        if (viewportPos.y > verticalFollowThreshold)
        {
            float excessViewport = viewportPos.y - verticalFollowThreshold;
            float worldHeight = cam.orthographicSize * 2f;
            targetY = transform.position.y + excessViewport * worldHeight;
        }

        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref velocityY, smoothTime);

        transform.position = new Vector3(newX, newY, transform.position.z);

        if (background != null)
        {
            background.transform.position = new Vector3(transform.position.x, backgroundDefaultY, backgroundDefaultZ);
        }
    }
}