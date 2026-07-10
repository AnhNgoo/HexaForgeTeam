using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("My Info")]
    [SerializeField] private TMP_Text myRankText;
    [SerializeField] private TMP_Text myScoreText;

    [Header("Content")]
    [SerializeField] private Transform content;

    [Header("Prefab")]
    [SerializeField] private GameObject leaderboardItemPrefab;

    private void Start()
    {
    }

    public void OpenPanel()
    {
        panel.SetActive(true);

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.LoadLeaderboard(this);
        }
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void SetMyInfo(
        int rank,
        int score)
    {
        myRankText.text = $"Rank #{rank}";
        myScoreText.text = $"Score : {score}";
    }

    public void ClearItems()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddItem(
        int rank,
        string playerName,
        int score)
    {
        GameObject item =
            Instantiate(
                leaderboardItemPrefab,
                content);

        LeaderboardItemUI itemUI =
            item.GetComponent<LeaderboardItemUI>();

        itemUI.Setup(
            rank,
            playerName,
            score);
    }
}