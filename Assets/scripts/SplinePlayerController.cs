using UnityEngine;
using HorseCity.Core;

public class SplinePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Lane Switching")]
    [SerializeField] private float laneSwitchCooldown = 0.3f;
    private float lastLaneSwitchTime;

    [Header("Components")]
    [SerializeField] private SplinePathManager pathManager;

    private InputActions inputActions;
    private bool isPlaying = false;

    private void Awake()
    {
        inputActions = new InputActions();
    
        if (pathManager == null)
        {
            pathManager = GetComponent<SplinePathManager>();
        }
    }

    void OnEnable()
    {
        inputActions.Enable();
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        inputActions.Disable();
    }

    void OnPlaying()
    {
        isPlaying = true;
        inputActions.Player.Enable();
    }

    void OnNotPlaying()
    {
        isPlaying = false;
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
        if (!isPlaying) return;
        if (pathManager == null) return;

        HandleLaneSwitchingInput();
        pathManager.MoveForward(Time.deltaTime);
        UpdatePositionAndRotation();
    }

    private void HandleLaneSwitchingInput()
    {
        bool switchLeft = inputActions.Player.SwitchLeft.ReadValue<float>() > 0.5f;
        bool switchRight = inputActions.Player.SwitchRight.ReadValue<float>() > 0.5f;

        if (inputActions.Player.Movement.ReadValue<Vector2>().x < -0.5f)
        {
            switchLeft = true;
        }
        else if (inputActions.Player.Movement.ReadValue<Vector2>().x > 0.5f)
        {
            switchRight = true;
        }

        // Check cooldown
        if (Time.time - lastLaneSwitchTime < laneSwitchCooldown) return;

        if (switchLeft)
        {
            pathManager.TrySwitchToLeftLane();
            lastLaneSwitchTime = Time.time;
        }
        else if (switchRight)
        {
            pathManager.TrySwitchToRightLane();
            lastLaneSwitchTime = Time.time;
        }
    }

    private void UpdatePositionAndRotation()
    {
        Vector3 targetPosition = pathManager.GetCurrentPosition();
        Vector3 targetDirection = pathManager.GetCurrentDirection();

        transform.position = targetPosition;

        // Update rotation to face the direction of movement
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
