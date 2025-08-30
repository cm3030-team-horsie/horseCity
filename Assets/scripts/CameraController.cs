using UnityEngine;
using HorseCity.Core;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    [SerializeField] private float followDistance = 8f;
    [SerializeField] private float heightOffset = 5.5f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;

    private Camera cam;
    private bool isFollowing = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = false;

        // store original transform/position
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

        Vector3 targetPosition = CalculateCameraPosition();
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // calc rotation to look at the player
        Vector3 lookDirection = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private Vector3 CalculateCameraPosition()
    {
        Vector3 playerForward = player.forward;
        playerForward.y = 0f;
        playerForward.Normalize();

        Vector3 behindPlayer = player.position - (playerForward * followDistance);
        Vector3 cameraPosition = behindPlayer + Vector3.up * heightOffset;
        return cameraPosition;
    }
}
