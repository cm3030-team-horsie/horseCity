using UnityEngine;
using HorseCity.Core;

public class PlayerController : MonoBehaviour
{
    private InputActions inputActions;

    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Y=0 plane

    private bool isPlaying = false;

    [SerializeField] private float moveSpeed = 5f;

    private void Awake()
    {
        inputActions = new InputActions();
    }

    // Unity lifecycle method. Automatically called when script- or game object is enabled
    void OnEnable()
    {
        inputActions.Enable();
        // Setup event listener
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    // Unity lifecycle method. Automatically called when script- or game object is disabled
    void OnDisable()
    {
        // Tear down event listener
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        inputActions.Disable();
    }

    void OnPlaying()
    {
        isPlaying = true;
        // Enable player controls
        inputActions.Player.Enable();
    }

    void OnNotPlaying()
    {
        isPlaying = false;
        // Disable player controls
        inputActions.Player.Disable();
    }

    void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                OnPlaying();
                break;
            default:
                OnNotPlaying();
                break;
        }
    }

    void Update()
    {
        if (isPlaying)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main camera not found");
                return;
            }
            // Vector3 mousePosition = Mouse.current.position.ReadValue();
                Vector2 mousePosition = inputActions.Player.MousePosition.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(mousePosition);

            // if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, raycastLayerMask))
            if (groundPlane.Raycast(ray, out float distance))
            {
                // Vector3 hitPoint = hitInfo.point;
                Vector3 targetPoint = ray.GetPoint(distance);
                MoveTowards(targetPoint);
            }
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0f;

        Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Rotate to face direction
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
    }
}
