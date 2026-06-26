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
        rankText.text =
            $"#{rank}";

        playerNameText.text =
            playerName;

        scoreText.text =
            score.ToString();
    }
}