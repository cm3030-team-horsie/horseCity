using UnityEngine;
using TMPro;

public class AppleCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI AppleText;

    private void OnEnable()
    {
        UpdateText(AppleCounter.Apples);
        AppleCounter.OnChanged += UpdateText;    // counter
    }

    private void OnDisable() => AppleCounter.OnChanged -= UpdateText;

    private void UpdateText(int value)
    {
        AppleText.text = $"Apples: {value}";
        // Debug.Log("UI update: " + value);
    }
}
