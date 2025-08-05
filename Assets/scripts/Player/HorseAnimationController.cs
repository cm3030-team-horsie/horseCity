using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HorseCity.Core;

class HorseAnimationController : MonoBehaviour
{
    private Animator animator;
    private SplineTraveler splineTraveler;
    private PlayerInputHandler playerInputHandler;

    [SerializeField]
    public bool showDebugInfo = false;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.0f;
    [SerializeField] private float targetSpeed = 0f; // The target speed we're transitioning to
    [SerializeField] private float currentSpeed = 0f; // The current speed value
    [SerializeField] private bool isTransitioning = false; // Whether we're currently transitioning

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the horse");
        }
        splineTraveler = GetComponent<SplineTraveler>();
        if (splineTraveler == null)
        {
            Debug.LogError("SplineTraveler component not found on the horse");
        }
        playerInputHandler = GetComponent<PlayerInputHandler>();
        if (playerInputHandler == null)
        {
            Debug.LogError("PlayerInputHandler component not found on the horse");
        }
    }

    public void Start()
    {
        splineTraveler.OnStartedMoving += OnStartedMoving;
        splineTraveler.OnStoppedMoving += OnStoppedMoving;

        playerInputHandler.OnJumpPerformed += OnJumpPerformed;

        if (showDebugInfo)
        {
            InvokeRepeating("LogCurrentAnimation", 0f, 1f);
        }

        currentSpeed = animator.GetFloat("Speed");
        targetSpeed = currentSpeed;
    }

    public void LogCurrentAnimation()
    {
        if (!showDebugInfo) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        string name = "";
        if (state.IsName("Idle"))
        {
            name = "Idle";
        }
        else if (state.IsName("Gait"))
        {
            name = "Gait";
        }
        else if (state.IsName("Jump"))
        {
            name = "Jump";
        }
        else
        {
            name = "Unknown";
        }

        float animatorSpeed = animator.GetFloat("Speed");
        string transitionStatus = isTransitioning ? " (transitioning)" : "";
        string message = $"Current animation: {name}, Speed parameter: {animatorSpeed:F2}{transitionStatus}";
        Debug.Log(message);
    }

    private void OnStartedMoving(SplineTraveler traveler)
    {
        string message = "Horse is moving, triggering movement animation";
        if (showDebugInfo)
        {
            Debug.Log(message);
        }

        StartSpeedTransition(1f, 0f);
    }

    private void OnStoppedMoving(SplineTraveler traveler)
    {
        string message = "Horse is stopped, triggering idle animation";
        if (showDebugInfo)
        {
            Debug.Log(message);
        }

        StartSpeedTransition(0f, 1f);
    }

    private void Update()
    {
        if (isTransitioning)
        {
            UpdateSpeedTransition();
        }
    }

    private void StartSpeedTransition(float newTargetSpeed, float newTransitionDuration)
    {
        targetSpeed = newTargetSpeed;
        transitionDuration = newTransitionDuration;
        isTransitioning = true;

        if (showDebugInfo)
        {
            Debug.Log($"Starting speed transition: {currentSpeed:F2} -> {targetSpeed:F2}");
        }
    }

    private void UpdateSpeedTransition()
    {
        float step = Time.deltaTime / transitionDuration;

        if (targetSpeed > currentSpeed)
        {
            currentSpeed = Mathf.Min(currentSpeed + step, targetSpeed);
        }
        else if (targetSpeed < currentSpeed)
        {
            currentSpeed = Mathf.Max(currentSpeed - step, targetSpeed);
        }

        animator.SetFloat("Speed", currentSpeed);

        if (Mathf.Approximately(currentSpeed, targetSpeed))
        {
            isTransitioning = false;
            if (showDebugInfo)
            {
                Debug.Log($"Speed transition complete: {currentSpeed:F2}");
            }
        }
    }

    private void OnJumpPerformed()
    {
        animator.SetTrigger("Jump");

        if (showDebugInfo)
        {
            Debug.Log("Horse jumped");
        }
    }

    private void OnDestroy()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnJumpPerformed -= OnJumpPerformed;
        }
    }
}
