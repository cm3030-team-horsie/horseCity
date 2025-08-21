using UnityEngine;
using HorseCity.Core;

class HorseAnimationController : MonoBehaviour
{
    private Animator animator;
    private SplineTraveler splineTraveler;
    private PlayerInputHandler playerInputHandler;

    [SerializeField] public bool showDebugInfo = false;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.0f;
    [SerializeField] private float targetSpeed = 0f; // The target speed we're transitioning to
    [SerializeField] private float currentSpeed = 0f; // The current speed value
    [SerializeField] private bool isTransitioning = false; // Whether we're currently transitioning

    [Header("Jump Settings")]
    [SerializeField] private float jumpBoostHeight = 1f;   // extra vertical height
    [SerializeField] private float jumpDuration = 0.6f;    // how long the boost lasts
    private bool isJumping = false;

    [Header("Start Sound")]
    [SerializeField] private AudioClip openTrailerSound;
    [SerializeField] private AudioClip gallopingSound;
    [SerializeField] private AudioClip jumpSound;

    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogError("AudioSource component not found on the horse");

    }

    public void Start()
    {
        splineTraveler.OnStartedMoving += OnStartedMoving;
        splineTraveler.OnStoppedMoving += OnStoppedMoving;

        playerInputHandler.OnJumpPerformed += OnJumpPerformed;

        // Listen for game over (lives = 0)
        if (LivesCounter.Instance != null)
            LivesCounter.Instance.OnGameOver += PlayDeathAnimation;

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
        if (state.IsName("Idle")) name = "Idle";
        else if (state.IsName("Gait")) name = "Gait";
        else if (state.IsName("Jump")) name = "Jump";
        else name = "Unknown";

        float animatorSpeed = animator.GetFloat("Speed");
        string transitionStatus = isTransitioning ? " (transitioning)" : "";
        Debug.Log($"Current animation: {name}, Speed parameter: {animatorSpeed:F2}{transitionStatus}");
    }

    private void PlayDeathAnimation()
    {
        Debug.Log("Horse died – playing death animation & stopping movement.");

        // stops the movement of the horse immediately
        if (splineTraveler != null)
        {
            splineTraveler.StopMoving();
            splineTraveler.enabled = false;
        }

        // no more player inpout - disabled
        if (playerInputHandler != null)
        {
            playerInputHandler.enabled = false;
        }

        // trigger the death animation
        animator.SetTrigger("Die");
    }

    private void OnStartedMoving(SplineTraveler traveler)
    {
        if (showDebugInfo) Debug.Log("Horse is moving, triggering movement animation");

        // play trailer bang sound once
        if (openTrailerSound != null && audioSource != null)
            audioSource.PlayOneShot(openTrailerSound);

        // start galloping sound
        if (gallopingSound != null && audioSource != null)
        {
            // delay for the open trailer soundd
            float delay = openTrailerSound != null ? openTrailerSound.length : 0f;
            Invoke(nameof(StartGallopingSound), delay);
        }

        StartSpeedTransition(1f, 0f);
    }

    private void StartGallopingSound()
    {
        if (gallopingSound != null && audioSource != null)
        {
            audioSource.clip = gallopingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }


    private void OnStoppedMoving(SplineTraveler traveler)
    {
        if (showDebugInfo) Debug.Log("Horse is stopped, triggering idle animation");
        StartSpeedTransition(0f, 1f);
    }

    private void Update()
    {
        if (isTransitioning) UpdateSpeedTransition();
    }

    private void StartSpeedTransition(float newTargetSpeed, float newTransitionDuration)
    {
        targetSpeed = newTargetSpeed;
        transitionDuration = newTransitionDuration;
        isTransitioning = true;

        if (showDebugInfo)
            Debug.Log($"Starting speed transition: {currentSpeed:F2} -> {targetSpeed:F2}");
    }

    private void UpdateSpeedTransition()
    {
        float step = Time.deltaTime / transitionDuration;

        if (targetSpeed > currentSpeed)
            currentSpeed = Mathf.Min(currentSpeed + step, targetSpeed);
        else if (targetSpeed < currentSpeed)
            currentSpeed = Mathf.Max(currentSpeed - step, targetSpeed);

        animator.SetFloat("Speed", currentSpeed);

        if (Mathf.Approximately(currentSpeed, targetSpeed))
        {
            isTransitioning = false;
            if (showDebugInfo)
                Debug.Log($"Speed transition complete: {currentSpeed:F2}");
        }
    }

    private void OnJumpPerformed()
    {
        if (!isJumping)
            StartCoroutine(JumpBoost());

        // jump sound
        if (jumpSound != null && audioSource != null)
            audioSource.PlayOneShot(jumpSound);

        if (showDebugInfo)
            Debug.Log("Horse jumped");
    }

    private System.Collections.IEnumerator JumpBoost()
    {
        isJumping = true;

        // trigger jump animation
        animator.SetTrigger("Jump");

        float elapsed = 0f;
        float baseY = transform.position.y;  // remember the horse’s ground level

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float normalized = elapsed / jumpDuration;

            // smooth jumparc
            float height = (1f - Mathf.Cos(normalized * Mathf.PI)) * jumpBoostHeight;


            // changes y position ONLY
            Vector3 pos = transform.position;
            pos.y = baseY + height;
            transform.position = pos;

            yield return null;
        }

        isJumping = false;
    }

    private void OnDestroy()
    {
        if (playerInputHandler != null)
            playerInputHandler.OnJumpPerformed -= OnJumpPerformed;

        if (LivesCounter.Instance != null)
            LivesCounter.Instance.OnGameOver -= PlayDeathAnimation;
    }

}