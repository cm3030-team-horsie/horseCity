using UnityEngine;
using UnityEngine.UI;
using HorseCity.Core;

public class EscapeTextUI : MonoBehaviour
{
    private Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<Text>();
    }

    void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
        UpdateTextVisibility(GameManager.Instance.GetGameState());
    }

    void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    void HandleGameStateChanged(GameState newState)
    {
        UpdateTextVisibility(newState);
    }

    void UpdateTextVisibility(GameState state)
    {
        bool shouldBeVisible = (state == GameState.WaitingToStart);

        if (textComponent != null)
        {
            textComponent.enabled = shouldBeVisible;
        }
    }
}
