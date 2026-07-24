using UnityEngine;
using TMPro;

public class RunResultSummary : MonoBehaviour
{
    public static RunResultSummary Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject summaryPanel; 

    [Header("Summary Texts")]
    [SerializeField] private TMP_Text txtStatsNotify; 
    [SerializeField] private TMP_Text txtRewards;     

    private int calculatedGem;
    private int calculatedExp;
    private int calculatedShards; 

    private void Awake()
    {
        Instance = this;
        if (summaryPanel != null) summaryPanel.SetActive(false); 
    }

    public void DisplaySummary(int normalKilled, int eliteKilled, int bossKilled)
    {
        if (summaryPanel == null) return;

        int totalKills = normalKilled + eliteKilled + bossKilled;
        int calculatedScore = (normalKilled * 100) + (eliteKilled * 300) + (bossKilled * 1000);

        calculatedGem = (normalKilled * 2) + (eliteKilled * 10) + (bossKilled * 50);
        calculatedExp = (normalKilled * 10) + (eliteKilled * 30) + (bossKilled * 100);
        calculatedShards = (normalKilled * 5) + (eliteKilled * 20) + (bossKilled * 150);

        int weaponShards = Mathf.Clamp(totalKills / 10, 1, 5) + (bossKilled * 2);

        if (txtStatsNotify != null)
        {
            txtStatsNotify.SetTextSafe($"<b><color=#FFCC00>VICTORY ACHIEVED</color></b>\n\n" +
                                      $"Normal Monsters: <color=#FFFFFF>{normalKilled}</color>\n" +
                                      $"Elite Monsters: <color=#FFCC00>{eliteKilled}</color>\n" +
                                      $"Boss Targets: <color=#FF3333>{bossKilled}</color>\n\n" +
                                      $"Total Score: <color=#FFFF66>{calculatedScore}</color>");
        }

        if (txtRewards != null)
        {
            txtRewards.SetTextSafe($"<b><color=#00FFCC>REWARDS ACQUIRED</color></b>\n\n" +
                                  $"- Crystals: <color=#33FFFF>+{calculatedGem}</color>\n" +
                                  $"- Rune Shards: <color=#CC66FF>+{calculatedShards}</color>\n" +
                                  $"- Account EXP: <color=#33FF33>+{calculatedExp}</color>\n" +
                                  $"- Weapon Shards: <color=#FFA500>+{weaponShards}</color>");
        }

        summaryPanel.SetActive(true);

        if (RunManager.Instance != null)
        {
            RunManager.Instance.SetPendingRewards(calculatedGem, calculatedExp, calculatedShards);
        }

        if (RuneInventoryManager.Instance != null)
        {
            RuneColor randomColor = (RuneColor)Random.Range(0, 3);
            RuneRarity randomRarity = RuneRarity.Common;
            
            if (bossKilled > 0) randomRarity = RuneRarity.Epic;
            else if (eliteKilled > 0) randomRarity = (Random.Range(0, 2) == 0) ? RuneRarity.Rare : RuneRarity.Common;

            RuneData newRune = new RuneData(randomColor, randomRarity)
            {
                runeName = $"Relic: {randomRarity} {randomColor}",
                runeLore = "An ancient relic recovered from the deep nightmare."
            };
            RuneInventoryManager.Instance.AddRune(newRune);
        }
    }

    public void OnConfirmAndReturn()
    {
        if (RunManager.Instance != null)
        {
            RunManager.Instance.ReturnToLobby();
        }
    }
}