using UnityEngine;
using TMPro;

public class RuneShardManager : MonoBehaviour
{
    public static RuneShardManager Instance;

    [Header("Shard Value")]
    [SerializeField] private int currentShards = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text shardText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ trình quản lý không bị hủy khi đổi sang map hầm ngục
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Nạp số lượng Shards hiện có từ file lưu lên bộ nhớ
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            currentShards = SaveLoadManager.Instance.SaveData.runeShards;
        }

        UpdateShardUI();
    }

    #region Add

    public void AddShards(int amount)
    {
        currentShards += amount;

        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.runeShards = currentShards;
            SaveLoadManager.Instance.SaveGame();
        }

        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.MarkDirty();
        }

        UpdateShardUI();
    }

    #endregion

    #region Spend

    public bool SpendShards(int amount)
    {
        if (currentShards < amount)
        {
            return false;
        }

        currentShards -= amount;

        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.runeShards = currentShards;
            SaveLoadManager.Instance.SaveGame();
        }

        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.MarkDirty();
        }

        UpdateShardUI();

        return true;
    }

    #endregion

    #region Get

    public int GetCurrentShards()
    {
        return currentShards;
    }

    #endregion

    #region UI

    public void UpdateShardUI()
    {
        if (shardText != null)
        {
            shardText.text = currentShards.ToString();
        }
    }

    #endregion
}