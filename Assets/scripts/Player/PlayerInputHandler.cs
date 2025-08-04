using UnityEngine;
using HorseCity.Core;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool enableInput = true;

    private InputActions inputActions;
    private LaneSwitcher laneSwitcher;
    private SplineTraveler splineTraveler;
    private bool isPlaying = false;

    public System.Action<PlayerInputHandler> OnInputEnabled;
    public System.Action<PlayerInputHandler> OnInputDisabled;

    private void Awake()
    {
        inputActions = new InputActions();

        laneSwitcher = GetComponent<LaneSwitcher>();
        if (laneSwitcher == null)
        {
            Debug.LogWarning($"PlayerInputHandler on {gameObject.name} requires a LaneSwitcher component");
        }

        splineTraveler = GetComponent<SplineTraveler>();
        if (splineTraveler == null)
        {
            Debug.LogWarning($"PlayerInputHandler on {gameObject.name} requires a SplineTraveler component");
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        inputActions.Disable();
    }

    private void Update()
    {
        if (!enableInput || !isPlaying) return;
        HandleLaneSwitchingInput();
    }

    private void HandleLaneSwitchingInput()
    {
        if (laneSwitcher == null) return;

        bool switchLeft = inputActions.Player.SwitchLeft.ReadValue<float>() > 0.5f;
        bool switchRight = inputActions.Player.SwitchRight.ReadValue<float>() > 0.5f;

        if (switchLeft)
        {
            laneSwitcher.TrySwitchToLeftLane();
        }
        else if (switchRight)
        {
            laneSwitcher.TrySwitchToRightLane();
        }
    }

    private void HandleGameStateChanged(GameState state)
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

    private void OnPlaying()
    {
        isPlaying = true;
        inputActions.Player.Enable();

        if (splineTraveler != null)
        {
            splineTraveler.StartMoving();
        }

        OnInputEnabled?.Invoke(this);
    }

    private void OnNotPlaying()
    {
        isPlaying = false;
        inputActions.Player.Disable();

        if (splineTraveler != null)
        {
            splineTraveler.StopMoving();
        }

        OnInputDisabled?.Invoke(this);
    }

    public void SetInputEnabled(bool enabled)
    {
        enableInput = enabled;
        if (enabled)
        {
            OnInputEnabled?.Invoke(this);
        }
        else
        {
            OnInputDisabled?.Invoke(this);
        }
    }

    public bool IsInputEnabled => enableInput && isPlaying;

    public bool IsPlaying => isPlaying;
}
