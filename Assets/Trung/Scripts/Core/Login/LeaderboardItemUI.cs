using TMPro;
using UnityEngine;

public class LeaderboardItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;

    public void Setup(
        int rank,
        string playerName,
        int score)
    {
        rankText.SetTextSafe(
            $"#{rank}");

        playerNameText.SetTextSafe(
            playerName);

        scoreText.SetTextSafe(
            score.ToString());
    }
}