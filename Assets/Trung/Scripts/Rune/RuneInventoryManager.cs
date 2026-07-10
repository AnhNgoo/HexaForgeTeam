using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuneInventoryManager : MonoBehaviour
{
    public static RuneInventoryManager Instance;

    public List<RuneData> runes = new List<RuneData>();

    [Header("Rune Hard Cap")]
    [SerializeField] private float maxHP = 2800f;
    [SerializeField] private float maxMP = 2800f;
    [SerializeField] private float maxStamina = 2800f;
    [SerializeField] private float maxATK = 110f;
    [SerializeField] private float maxDEF = 75f;
    [SerializeField] private float maxHPPercent = 28f;
    [SerializeField] private float maxMPPercent = 28f;
    [SerializeField] private float maxStaminaPercent = 28f;
    [SerializeField] private float maxATKPercent = 24f;
    [SerializeField] private float maxDEFPercent = 24f;
    [SerializeField] private float maxCritChance = 38f;
    [SerializeField] private float maxCritDamage = 90f;
    [SerializeField] private float maxArmorPenetration = 28f;
    [SerializeField] private float maxStaminaRegen = 45f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadRunes();
    }

    public void AddRune(RuneData runeData)
    {
        runes.Add(runeData);
        SaveRunes();
    }

    public void RemoveRune(string runeID)
    {
        for (int i = runes.Count - 1; i >= 0; i--)
        {
            if (runes[i].runeID == runeID)
            {
                runes.RemoveAt(i);
                break;
            }
        }

        SaveRunes();
    }

    #region Equip

    public bool EquipRune(RuneData runeData, CharacterType targetCharType)
    {
        if (runeData == null)
        {
            return false;
        }

        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(targetCharType);
        if (build == null)
        {
            return false;
        }

        // BƯỚC BẢO VỆ 1: Quét dọn data ma (Ghost Data) trước khi xử lý
        // Nếu slot nào chứa ID ngọc không còn tồn tại trong túi, lập tức dọn sạch ô đó thành trống ""
        for (int i = 0; i < build.equippedRuneIDs.Length; i++)
        {
            string id = build.equippedRuneIDs[i];
            if (!string.IsNullOrEmpty(id))
            {
                bool exists = runes.Any(r => r.runeID == id);
                if (!exists)
                {
                    build.equippedRuneIDs[i] = ""; // Trả lại ô trống chuẩn
                }
            }
        }

        // Kiểm tra xem viên ngọc này đã được chính nhân vật này đeo ở ô khác chưa
        for (int i = 0; i < build.equippedRuneIDs.Length; i++)
        {
            if (build.equippedRuneIDs[i] == runeData.runeID)
            {
                return false;
            }
        }

        int emptySlot = -1;
        if (RuneEquipUI.Instance != null)
        {
            bool isUltimate = false;
            for (int j = 0; j < runeData.affixes.Count; j++)
            {
                if (runeData.affixes[j].statType == RuneStatType.AllStats) { isUltimate = true; break; }
            }

            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                if (string.IsNullOrEmpty(build.equippedRuneIDs[i]))
                {
                    if (isUltimate)
                    {
                        emptySlot = i;
                        break;
                    }
                    
                    RuneColor requiredColor = RuneEquipUI.Instance.GetSlotRequiredColor(targetCharType, i);
                    if (runeData.runeColor == requiredColor)
                    {
                        emptySlot = i;
                        break;
                    }
                }
            }
        }

        if (emptySlot == -1)
        {
            return false;
        }

        build.equippedRuneIDs[emptySlot] = runeData.runeID;
        SaveRunes();

        if (LobbyStatManager.Instance != null)
        {
            LobbyStatManager.Instance.RecalculateStats();
        }
        
        if (RuneEquipUI.Instance != null)
        {
            RuneEquipUI.Instance.RefreshEquipUI();
        }

        return true;
    }

    public void UnequipRune(RuneData runeData, CharacterType targetCharType)
    {
        if (runeData == null)
        {
            return;
        }

        // Đã sửa: Tìm đúng bảng ngọc của nhân vật được yêu cầu
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(targetCharType);

        if (build == null)
        {
            return;
        }

        // Tìm và xóa ID viên ngọc ra khỏi danh sách trang bị của nhân vật này
        for (int i = 0; i < build.equippedRuneIDs.Length; i++)
        {
            if (build.equippedRuneIDs[i] == runeData.runeID)
            {
                build.equippedRuneIDs[i] = "";
                break;
            }
        }

        SaveRunes();

        if (LobbyStatManager.Instance != null)
        {
            LobbyStatManager.Instance.RecalculateStats();
        }    

        if (RuneEquipUI.Instance != null)
{
    RuneEquipUI.Instance.RefreshEquipUI();
}

Debug.Log($"<color=#FFFF66><b>[THÁO NGỌC]</b> Thực hiện gỡ viên ngọc {runeData.runeName} khỏi nhân vật {targetCharType.ToString().ToUpper()} thành công.</color>");
if (LobbyNotifyManager.Instance != null) 
    LobbyNotifyManager.Instance.ShowNotify($"Rune unequipped from {targetCharType.ToString()}.", Color.white);
    }

    #endregion

    #region Total Stats

    // Đã sửa: Tính toán chỉ số theo nhân vật được truyền vào
    public Dictionary<RuneStatType, float> GetStats(CharacterType targetCharType)
    {
        Dictionary<RuneStatType, float> normalStats = new Dictionary<RuneStatType, float>();
        Dictionary<RuneStatType, float> bypassStats = new Dictionary<RuneStatType, float>();

        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(targetCharType);

        if (build != null)
        {
            // Quét qua 3 Slot ID ngọc của nhân vật
            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                string targetID = build.equippedRuneIDs[i];
                if (string.IsNullOrEmpty(targetID))
                {
                    continue;
                }

                // Tìm viên ngọc thực tế trong túi đồ thông qua ID
                RuneData rune = null;
                for (int k = 0; k < runes.Count; k++)
                {
                    if (runes[k].runeID == targetID)
                    {
                        rune = runes[k];
                        break;
                    }
                }

                if (rune == null)
                {
                    continue;
                }

                Dictionary<RuneStatType, float> targetDict = rune.ignoreHardCap ? bypassStats : normalStats;

                for (int j = 0; j < rune.affixes.Count; j++)
                {
                    RuneAffixData affix = rune.affixes[j];

                    if (!targetDict.ContainsKey(affix.statType))
                    {
                        targetDict.Add(affix.statType, 0f);
                    }

                    targetDict[affix.statType] += affix.value;
                }
            }
        }

        ApplyRuneHardCaps(normalStats);

        foreach (var stat in bypassStats)
        {
            if (!normalStats.ContainsKey(stat.Key))
            {
                normalStats.Add(stat.Key, 0f);
            }

            normalStats[stat.Key] += stat.Value;
        }

        return normalStats;
    }

    [ContextMenu("Debug Total Stats")]
    private void DebugTotalStats()
    {
        Dictionary<RuneStatType, float> stats = GetStats(CharacterManager.Instance.GetSelectedCharacter());

        foreach (var stat in stats)
        {
            Debug.Log($"{stat.Key} : {stat.Value}");
        }
    }

    #endregion

    private void ApplyRuneHardCaps(Dictionary<RuneStatType, float> stats)
    {
        ClampStat(stats, RuneStatType.HP, maxHP);
        ClampStat(stats, RuneStatType.MP, maxMP);
        ClampStat(stats, RuneStatType.Stamina, maxStamina);
        ClampStat(stats, RuneStatType.ATK, maxATK);
        ClampStat(stats, RuneStatType.DEF, maxDEF);
        ClampStat(stats, RuneStatType.HPPercent, maxHPPercent);
        ClampStat(stats, RuneStatType.MPPercent, maxMPPercent);
        ClampStat(stats, RuneStatType.StaminaPercent, maxStaminaPercent);
        ClampStat(stats, RuneStatType.ATKPercent, maxATKPercent);
        ClampStat(stats, RuneStatType.DEFPercent, maxDEFPercent);
        ClampStat(stats, RuneStatType.CritChance, maxCritChance);
        ClampStat(stats, RuneStatType.CritDamage, maxCritDamage);
        ClampStat(stats, RuneStatType.ArmorPenetration, maxArmorPenetration);
        ClampStat(stats, RuneStatType.StaminaRegen, maxStaminaRegen);
    }

    private void ClampStat(Dictionary<RuneStatType, float> stats, RuneStatType statType, float maxValue)
    {
        if (!stats.ContainsKey(statType))
        {
            return;
        }

        stats[statType] = Mathf.Min(stats[statType], maxValue);
    }

    public float GetHardCap(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP: return maxHP;
            case RuneStatType.MP: return maxMP;
            case RuneStatType.Stamina: return maxStamina;
            case RuneStatType.ATK: return maxATK;
            case RuneStatType.DEF: return maxDEF;
            case RuneStatType.HPPercent: return maxHPPercent;
            case RuneStatType.MPPercent: return maxMPPercent;
            case RuneStatType.StaminaPercent: return maxStaminaPercent;
            case RuneStatType.ATKPercent: return maxATKPercent;
            case RuneStatType.DEFPercent: return maxDEFPercent;
            case RuneStatType.CritChance: return maxCritChance;
            case RuneStatType.CritDamage: return maxCritDamage;
            case RuneStatType.ArmorPenetration: return maxArmorPenetration;
            case RuneStatType.StaminaRegen: return maxStaminaRegen;
        }
        return 0f;
    }

    private void LoadRunes()
    {
        runes = SaveLoadManager.Instance.SaveData.runes;

        if (runes == null)
        {
            runes = new List<RuneData>();
        }
    }

    private void SaveRunes()
    {
        SaveLoadManager.Instance.SaveData.runes = runes;
        SaveLoadManager.Instance.SaveGame();

        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.SaveCloud();
        }
        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.MarkDirty();
        }
    }
}