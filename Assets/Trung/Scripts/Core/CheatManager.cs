using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public static CheatManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HackGems()
    {
        if (GemManager.Instance != null)
        {
            GemManager.Instance.AddGem(5000);
            Debug.Log("<color=#FFD700><b>[CHEAT]</b> Command executed: Granted +5,000 Crystals to account.</color>");
        }
        else
        {
            Debug.LogError("[CHEAT] GemManager target instance not found in current scene context.");
        }
    }

    public void HackRuneShards()
    {
        if (RuneShardManager.Instance != null)
        {
            RuneShardManager.Instance.AddShards(2000);
            Debug.Log("<color=#CC66FF><b>[CHEAT]</b> Command executed: Granted +2,000 Rune Shards to account.</color>");
        }
        else
        {
            Debug.LogError("[CHEAT] RuneShardManager target instance not found in current scene context.");
        }
    }

    public void HackExperience()
    {
        if (AccountLevelManager.Instance != null)
        {
            AccountLevelManager.Instance.AddExp(1000);
            Debug.Log("<color=#00FFCC><b>[CHEAT]</b> Command executed: Granted +1,000 Account Experience.</color>");

            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.CheckUnlockCharacter();
            }
        }
        else
        {
            Debug.LogError("[CHEAT] AccountLevelManager target instance not found in current scene context.");
        }
    }

    public void HackSkipAllAchievements()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.HackUnlockAllAchievements();
        }
        else
        {
            Debug.LogError("[CHEAT] AchievementManager target instance not found.");
        }
    }
    // Thêm hàm này vào CheatManager.cs
    public void HackAddKillsAndRuns()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            var data = SaveLoadManager.Instance.SaveData;
            
            data.totalKills += 100; // Cộng 100 quái
            data.totalRuns += 5;    // Cộng 5 lượt đi

            SaveLoadManager.Instance.SaveGame();

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.UpdateAllStatistics();
            }

            Debug.Log("<color=#00FFCC><b>[CHEAT]</b> Đã cộng 100 Kills & 5 Runs!</color>");
        }
    }
}