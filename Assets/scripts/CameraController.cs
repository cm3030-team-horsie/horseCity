using UnityEngine;
using HorseCity.Core;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 5.5f, -8f);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float fixedAngleX = 30f;

    private Camera cam;
    private bool isFollowing = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = false;

        // Store the original transform (set in inspector)
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }
    
    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.WaitingToStart:
                OnWaitingToStart();
                break;
            case GameState.Playing:
                OnPlaying();
                break;
            default:
                OnNotPlaying();
                break;
        }
    }

    private void OnWaitingToStart()
    {
        isFollowing = false;
    }

    private void OnPlaying()
    {
        isFollowing = true;
    }

    private void OnNotPlaying()
    {
        isFollowing = false;
    }

    private void LateUpdate()
    {
        if (!isFollowing || player == null) return;

        Vector3 targetPosition = player.position + followOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        float targetY = player.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(fixedAngleX, targetY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
