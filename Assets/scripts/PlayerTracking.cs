using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerTracking : MonoBehaviour
{
    public Transform player;
    private Vector3 offset = new Vector3(8f, 8f, -8f);

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        cam.orthographic = true;
        cam.orthographicSize = 10f;

        transform.rotation = Quaternion.Euler(30f, -45f, 0f);
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}
