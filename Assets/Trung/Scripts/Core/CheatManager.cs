using UnityEngine;
using Sirenix.OdinInspector;
public class CheatManager : MonoBehaviour
{
    public static CheatManager Instance;

    [Header("UI Toggle Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private GameObject cheatPanelRoot;

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
            return;
        }

        if (cheatPanelRoot == null)
        {
            cheatPanelRoot = gameObject;
        }
    }

    private void Start()
    {
        // Luôn ẩn bảng Cheat Panel khi mới mở game
        if (cheatPanelRoot != null)
        {
            cheatPanelRoot.SetActive(false);
        }
    }

    private void Update()
    {
        // Nhấn F1 để bật / tắt bảng Cheat
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleCheatPanel();
        }
    }

    public void ToggleCheatPanel()
    {
        if (cheatPanelRoot != null)
        {
            bool currentState = cheatPanelRoot.activeSelf;
            cheatPanelRoot.SetActive(!currentState);

            if (LobbyNotifyManager.Instance != null && !currentState)
            {
                LobbyNotifyManager.Instance.ShowNotify("Debug Cheat Panel Opened", Color.cyan);
            }
        }
    }

    #region Hack Functions
    [Button("Hack Gems +5,000")]
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

    [Button("Hack Rune Shards +2,000")]
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

    [Button("Hack Experience +1,000")]
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

    [Button("Hack Skip All Achievements")]
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

    [Button("Hack Add Kills and Runs")]
    public void HackAddKillsAndRuns()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            var data = SaveLoadManager.Instance.SaveData;

            data.totalKills += 100;
            data.totalRuns += 5;

            SaveLoadManager.Instance.SaveGame();

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.UpdateAllStatistics();
            }

            Debug.Log("<color=#00FFCC><b>[CHEAT]</b> Command executed: Added +100 Kills & +5 Runs!</color>");
        }
    }

    #endregion
}