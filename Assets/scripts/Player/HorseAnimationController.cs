using UnityEngine;
using HorseCity.Core;

class HorseAnimationController : MonoBehaviour
{
    private Animator animator;
    private SplineTraveler splineTraveler;
    private PlayerInputHandler playerInputHandler;
    private LaneSwitcher laneSwitcher;

    [Header("UI References")]
    [SerializeField] private GameOverUI gameOverUI;

    [SerializeField] public bool showDebugInfo = false;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.0f;
    [SerializeField] private float targetSpeed = 0f;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private bool isTransitioning = false;

    [Header("Jump Settings")]
    [SerializeField] private float jumpBoostHeight = 1f;
    [SerializeField] private float jumpDuration = 0.6f;
    private bool isJumping = false;

    [Header("Start Sound")]
    [SerializeField] private AudioClip openTrailerSound;
    [SerializeField] private AudioClip gallopingSound;
    [SerializeField] private AudioClip jumpSound;

    private AudioSource audioSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        splineTraveler = GetComponent<SplineTraveler>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
        laneSwitcher = GetComponent<LaneSwitcher>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Start()
    {
        splineTraveler.OnStartedMoving += OnStartedMoving;
        splineTraveler.OnStoppedMoving += OnStoppedMoving;
        playerInputHandler.OnJumpPerformed += OnJumpPerformed;

        if (LivesCounter.Instance != null)
            LivesCounter.Instance.OnGameOver += PlayDeathAnimation;

        if (showDebugInfo)
            InvokeRepeating("LogCurrentAnimation", 0f, 1f);

        currentSpeed = animator.GetFloat("Speed");
        targetSpeed = currentSpeed;

        ApplyDifficultySettings();
    }

    private void ApplyDifficultySettings()
    {
        if (GameManager.CurrentDifficulty == Difficulty.Easy)
        {
            splineTraveler.TravelSpeed = 10f;
            if (laneSwitcher != null) laneSwitcher.SwitchDuration = 0.5f;
        }
        else if (GameManager.CurrentDifficulty == Difficulty.Hard)
        {
            splineTraveler.TravelSpeed = 20f;
            if (laneSwitcher != null) laneSwitcher.SwitchDuration = 1.0f;
        }
    }

    public void LogCurrentAnimation()
    {
        if (!showDebugInfo) return;
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        string name = state.IsName("Idle") ? "Idle" :
                      state.IsName("Gait") ? "Gait" :
                      state.IsName("Jump") ? "Jump" : "Unknown";
        float animatorSpeed = animator.GetFloat("Speed");
        Debug.Log($"Current animation: {name}, Speed: {animatorSpeed:F2}");
    }

    private void PlayDeathAnimation()
    {
        if (splineTraveler != null)
        {
            splineTraveler.StopMoving();
            splineTraveler.enabled = false;
        }
        if (playerInputHandler != null)
            playerInputHandler.enabled = false;

        animator.SetTrigger("Die");
        Invoke(nameof(ShowGameOverPanel), 2f);
    }

    private void ShowGameOverPanel()
    {
        if (gameOverUI != null) gameOverUI.Show();
    }

    private void OnStartedMoving(SplineTraveler traveler)
    {
        if (openTrailerSound != null) audioSource?.PlayOneShot(openTrailerSound);
        if (gallopingSound != null)
        {
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

    private void OnStoppedMoving(SplineTraveler traveler) =>
        StartSpeedTransition(0f, 1f);

    private void Update()
    {
        if (isTransitioning) UpdateSpeedTransition();
    }

    private void StartSpeedTransition(float newTargetSpeed, float newTransitionDuration)
    {
        targetSpeed = newTargetSpeed;
        transitionDuration = newTransitionDuration;
        isTransitioning = true;
    }

    private void UpdateSpeedTransition()
    {
        float step = Time.deltaTime / transitionDuration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, step);
        animator.SetFloat("Speed", currentSpeed);
        if (Mathf.Approximately(currentSpeed, targetSpeed)) isTransitioning = false;
    }

    private void OnJumpPerformed()
    {
        if (!isJumping) StartCoroutine(JumpBoost());
        if (jumpSound != null) audioSource?.PlayOneShot(jumpSound);
    }

    private System.Collections.IEnumerator JumpBoost()
    {
        isJumping = true;
        animator.SetTrigger("Jump");

        float elapsed = 0f;
        float baseY = transform.position.y;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float normalized = elapsed / jumpDuration;
            float height = (1f - Mathf.Cos(normalized * Mathf.PI)) * jumpBoostHeight;
            Vector3 pos = transform.position;
            pos.y = baseY + height;
            transform.position = pos;
            yield return null;
        }
        isJumping = false;
    }

    private void OnDestroy()
    {
        playerInputHandler.OnJumpPerformed -= OnJumpPerformed;
        if (LivesCounter.Instance != null)
            LivesCounter.Instance.OnGameOver -= PlayDeathAnimation;
    }
}