using UnityEngine;

public class LivesCounter : MonoBehaviour
{
    public static LivesCounter Instance;

    [Header("Lives Settings")]
    [SerializeField] private int startLives = 8;    // lives start with
    private int currentLives;

    public System.Action<int> OnLivesChanged; // uodate the UI
    public System.Action OnGameOver;           // game over

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentLives = startLives;
    }

    public void DeductLife()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            Debug.Log("Game Over");
            OnGameOver?.Invoke();
        }
    }

    public int GetLives()
    {
        return currentLives;
    }
}

