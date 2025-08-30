using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LivesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;

    private void Start()
    {
        if (LivesCounter.Instance != null)
        {
            // initialize text with starting points
            livesText.text = "Lives: " + LivesCounter.Instance.GetLives();

            // subscribe to updates
            LivesCounter.Instance.OnLivesChanged += UpdateLivesUI;
        }
    }

    private void OnDestroy()
    {
        if (LivesCounter.Instance != null)
        {
            LivesCounter.Instance.OnLivesChanged -= UpdateLivesUI;
        }
    }

    private void UpdateLivesUI(int newLives)
    {
        livesText.text = "Lives: " + newLives;
    }
}