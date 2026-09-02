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
    [SerializeField] private float maxStaminaRegen = 45f;
    [SerializeField] private float maxMPRegen = 25f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float maxPoisonDamage = 50f;

    private float lastSaveTime = -99f;

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

    #region Dismantle System (Chuẩn GDD)

    public void DismantleRunes(List<string> runeIDsToDismantle)
    {
        if (runeIDsToDismantle == null || runeIDsToDismantle.Count == 0) return;

        int totalGemsGained = 0;
        int totalShardsGained = 0;
        int countDismantled = 0;

        for (int i = runes.Count - 1; i >= 0; i--)
        {
            if (runeIDsToDismantle.Contains(runes[i].runeID))
            {
                RuneRarity rarity = runes[i].runeRarity;

                // Tỷ lệ hoàn trả Gem & Shard theo GDD
                switch (rarity)
                {
                    case RuneRarity.Common:
                        totalGemsGained += 10;
                        totalShardsGained += 50;
                        break;
                    case RuneRarity.Rare:
                        totalGemsGained += 25;
                        totalShardsGained += 150;
                        break;
                    case RuneRarity.Epic:
                        totalGemsGained += 60;
                        totalShardsGained += 400;
                        break;
                    case RuneRarity.Legendary:
                        totalGemsGained += 150;
                        totalShardsGained += 1000;
                        break;
                }

                runes.RemoveAt(i);
                countDismantled++;
            }
        }

        if (countDismantled > 0)
        {
            if (GemManager.Instance != null && totalGemsGained > 0)
            {
                GemManager.Instance.AddGem(totalGemsGained);
            }

            if (RuneShardManager.Instance != null && totalShardsGained > 0)
            {
                RuneShardManager.Instance.AddShards(totalShardsGained);
            }

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.AddDismantleProgress(countDismantled);
            }

            SaveRunes();

            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Dismantled {countDismantled} Runes! +{totalGemsGained} Gems, +{totalShardsGained} Shards", Color.green);
            }

            if (RuneInventoryUI.Instance != null)
            {
                RuneInventoryUI.Instance.RefreshInventory();
            }
        }
    }

    #endregion

    #region Equip

    public bool EquipRune(RuneData runeData, CharacterType targetCharType)
    {
        if (runeData == null)
        {
            return false;
        }

        if (CharacterManager.Instance != null)
        {
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                var checkBuild = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                if (checkBuild != null && checkBuild.equippedRuneIDs != null)
                {
                    for (int i = 0; i < checkBuild.equippedRuneIDs.Length; i++)
                    {
                        if (checkBuild.equippedRuneIDs[i] == runeData.runeID)
                        {
                            Debug.LogWarning($"[RuneInventoryManager] Chặn đeo: Ngọc {runeData.runeName} đang được {charType} sử dụng!");
                            return false;
                        }
                    }
                }
            }
        }

        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(targetCharType);
        if (build == null)
        {
            return false;
        }

        for (int i = 0; i < build.equippedRuneIDs.Length; i++)
        {
            string id = build.equippedRuneIDs[i];
            if (!string.IsNullOrEmpty(id))
            {
                bool exists = runes.Any(r => r.runeID == id);
                if (!exists)
                {
                    build.equippedRuneIDs[i] = "";
                }
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

        if (AchievementManager.Instance != null)
        {
            int equippedCount = 0;
            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                if (!string.IsNullOrEmpty(build.equippedRuneIDs[i])) equippedCount++;
            }
            AchievementManager.Instance.CheckEquipFullProgress(equippedCount);
        }

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
        if (runeData == null) return;

        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(targetCharType);
        if (build == null) return;

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

        if (LobbyNotifyManager.Instance != null) 
            LobbyNotifyManager.Instance.ShowNotify($"Rune unequipped from {targetCharType}.", Color.white);
    }

    #endregion

    #region Total Stats

    public Dictionary<RuneStatType, float> GetStats(CharacterType targetCharType)
    {
        Dictionary<RuneStatType, float> normalStats = new Dictionary<RuneStatType, float>();
        Dictionary<RuneStatType, float> bypassStats = new Dictionary<RuneStatType, float>();

        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(targetCharType);

        if (build != null)
        {
            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                string targetID = build.equippedRuneIDs[i];
                if (string.IsNullOrEmpty(targetID)) continue;

                RuneData rune = runes.FirstOrDefault(k => k.runeID == targetID);
                if (rune == null) continue;

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
        ClampStat(stats, RuneStatType.StaminaRegen, maxStaminaRegen);
        ClampStat(stats, RuneStatType.MPRegen, maxMPRegen);
        ClampStat(stats, RuneStatType.Speed, maxSpeed);
        ClampStat(stats, RuneStatType.PoisonDamage, maxPoisonDamage);
    }

    private void ClampStat(Dictionary<RuneStatType, float> stats, RuneStatType statType, float maxValue)
    {
        if (!stats.ContainsKey(statType)) return;
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
            case RuneStatType.StaminaRegen: return maxStaminaRegen;
            case RuneStatType.MPRegen: return maxMPRegen;
            case RuneStatType.Speed: return maxSpeed;
            case RuneStatType.PoisonDamage: return maxPoisonDamage;
        }
        return 0f;
    }

    private void LoadRunes()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            runes = SaveLoadManager.Instance.SaveData.runes;
        }

        if (runes == null)
        {
            runes = new List<RuneData>();
        }
    }

    public void SaveRunes()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.runes = runes;

            // Giới hạn tần suất lưu để chống spam API PlayFab
            if (Time.time - lastSaveTime > 2.0f)
            {
                lastSaveTime = Time.time;
                SaveLoadManager.Instance.SaveGame();

                if (PlayFabDataManager.Instance != null)
                {
                    PlayFabDataManager.Instance.MarkDirty();
                }
            }
        }
    }

    public void RemoveRunesRange(List<string> runeIDs)
    {
        if (runeIDs == null || runeIDs.Count == 0) return;

        bool anyRemoved = false;
        foreach (string id in runeIDs)
        {
            RuneData r = runes.FirstOrDefault(x => x.runeID == id);
            if (r != null)
            {
                runes.Remove(r);
                anyRemoved = true;
            }
        }
        if (anyRemoved)
        {
            SaveRunes();
        }
    }
}