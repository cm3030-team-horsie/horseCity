using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI applesCollectedText;

    private void Awake()
    {
        // hide panel at game start
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    public void ShowResults(int applesCollected)
    {
        if (resultsPanel == null)
        {
            return;
        }

        resultsPanel.SetActive(true);

        // decides rank
        string rank;
        if (applesCollected >= 20)
            rank = "<sprite name=goldMedal> GOLD";
        else if (applesCollected >= 12)
            rank = "<sprite name=silverMedal> SILVER";
        else if (applesCollected >= 8)
            rank = "<sprite name=bronzeMedal> BRONZE";
        else
            rank = "<sprite name=placedMedal> PLACED";

        // uodate the UI
        if (rankText != null)
            rankText.text = rank;

        if (applesCollectedText != null)
            applesCollectedText.text = $"<sprite name=apple> {applesCollected}";

        Debug.Log($"[ResultsUI] Showing results → Apples={applesCollected}, Rank={rank}");
    }
}
