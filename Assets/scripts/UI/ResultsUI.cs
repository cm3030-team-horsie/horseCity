//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;

//public class ResultsUI : MonoBehaviour
//{
//    [Header("UI References")]
//    [SerializeField] private GameObject resultsPanel;
//    [SerializeField] private TextMeshProUGUI rankText;
//    [SerializeField] private TextMeshProUGUI applesCollectedText;

//    public void ShowResults(int applesCollected)
//    {
//        resultsPanel.SetActive(true);

//        // Determine rank
//        string rank;
//        if (applesCollected >= 20)
//            rank = "Gold";
//        else if (applesCollected >= 12)
//            rank = "Silver";
//        else if (applesCollected >= 8)
//            rank = "Bronze";
//        else
//            rank = "Placed";

//        rankText.text = $"🏆 Rank: {rank}";
//        applesCollectedText.text = $"🍎 Apples Collected: {applesCollected}";
//    }
//}

using UnityEngine;
using TMPro;

public class ResultsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resultsPanel;              // drag ResultsPanel here
    [SerializeField] private TextMeshProUGUI rankText;             // drag RankText TMP here
    [SerializeField] private TextMeshProUGUI applesCollectedText;  // drag ApplesCollectedText TMP here

    private void Awake()
    {
        // 👇 Hide panel at game start (even though it's active in Inspector)
        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    public void ShowResults(int applesCollected)
    {
        if (resultsPanel == null)
        {
            Debug.LogError("[ResultsUI] Results Panel is not assigned!");
            return;
        }

        resultsPanel.SetActive(true); // run is complete

        string rank;
        if (applesCollected >= 20)
            rank = "Gold";
        else if (applesCollected >= 12)
            rank = "Silver";
        else if (applesCollected >= 8)
            rank = "Bronze";
        else
            rank = "Placed";

        if (rankText != null)
            rankText.text = $"{rank}";
        if (applesCollectedText != null)
            applesCollectedText.text = $"Apples Collected: {applesCollected}";

        Debug.Log($"[ResultsUI] Showing results → Apples={applesCollected}, Rank={rank}");
    }
}
