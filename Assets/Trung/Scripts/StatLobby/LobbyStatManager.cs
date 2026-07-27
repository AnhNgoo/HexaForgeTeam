using System.Collections.Generic;
using UnityEngine;

public class LobbyStatManager : MonoBehaviour
{
    public static LobbyStatManager Instance;

    public LobbyStatData BonusStats = new LobbyStatData();

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
    }

    private void Start()
    {
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        ResetBonusStats();
        ApplyAccountLevel();
        ApplyRuneStats();
        DebugBonusStats();

        PlayerStatTest stat = FindFirstObjectByType<PlayerStatTest>();
        if (stat != null)
        {
            stat.RecalculateStats();
        }
    }

    private void ResetBonusStats()
    {
        BonusStats.HP = 0; BonusStats.HPPercent = 0;
        BonusStats.MP = 0; BonusStats.MPPercent = 0;
        BonusStats.Stamina = 0; BonusStats.StaminaPercent = 0;
        BonusStats.ATK = 0; BonusStats.ATKPercent = 0;
        BonusStats.DEF = 0; BonusStats.DEFPercent = 0;
        BonusStats.CritChance = 0; BonusStats.CritDamage = 0;
        BonusStats.ArmorPenetration = 0; BonusStats.StaminaRegen = 0;
    }

    private void ApplyAccountLevel()
    {
        if (AccountLevelManager.Instance == null) return;

        BonusStats.HP += AccountLevelManager.Instance.GetHPBonus();
        BonusStats.ATK += AccountLevelManager.Instance.GetATKBonus();
    }

    private void ApplyRuneStats()
    {
        if (RuneInventoryManager.Instance == null) return;

        // Đã sửa: Truyền chuẩn nhân vật sảnh để lấy thuộc tính thực tế áp dụng lên Player
        CharacterType lobbyChar = CharacterManager.Instance.GetSelectedCharacter();
        Dictionary<RuneStatType, float> stats = RuneInventoryManager.Instance.GetStats(lobbyChar);

        foreach (var stat in stats)
        {
            switch (stat.Key)
            {
                case RuneStatType.HP: BonusStats.HP += stat.Value; break;
                case RuneStatType.HPPercent: BonusStats.HPPercent += stat.Value; break;
                case RuneStatType.MP: BonusStats.MP += stat.Value; break;
                case RuneStatType.MPPercent: BonusStats.MPPercent += stat.Value; break;
                case RuneStatType.Stamina: BonusStats.Stamina += stat.Value; break;
                case RuneStatType.StaminaPercent: BonusStats.StaminaPercent += stat.Value; break;
                case RuneStatType.ATK: BonusStats.ATK += stat.Value; break;
                case RuneStatType.ATKPercent: BonusStats.ATKPercent += stat.Value; break;
                case RuneStatType.DEF: BonusStats.DEF += stat.Value; break;
                case RuneStatType.DEFPercent: BonusStats.DEFPercent += stat.Value; break;
                case RuneStatType.CritChance: BonusStats.CritChance += stat.Value; break;
                case RuneStatType.CritDamage: BonusStats.CritDamage += stat.Value; break;
                case RuneStatType.ArmorPenetration: BonusStats.ArmorPenetration += stat.Value; break;
                case RuneStatType.StaminaRegen: BonusStats.StaminaRegen += stat.Value; break;
                case RuneStatType.AllStats:
                    BonusStats.HP += stat.Value;
                    BonusStats.MP += stat.Value;
                    BonusStats.Stamina += stat.Value;
                    BonusStats.ATK += stat.Value;
                    BonusStats.DEF += stat.Value;
                    BonusStats.CritChance += stat.Value;
                    BonusStats.CritDamage += stat.Value;
                    BonusStats.ArmorPenetration += stat.Value;
                    BonusStats.StaminaRegen += stat.Value;
                    break;
            }
        }
    }

    public LobbyStatData GetBonusStats() => BonusStats;

    private void DebugBonusStats()
    {
        Debug.Log(
            "===== LOBBY BONUS =====\n" +
            $"HP +{BonusStats.HP} | HP% +{BonusStats.HPPercent}%\n" +
            $"MP +{BonusStats.MP} | MP% +{BonusStats.MPPercent}%\n" +
            $"Stamina +{BonusStats.Stamina} | Stamina% +{BonusStats.StaminaPercent}%\n" +
            $"ATK +{BonusStats.ATK} | ATK% +{BonusStats.ATKPercent}%\n" +
            $"DEF +{BonusStats.DEF} | DEF% +{BonusStats.DEFPercent}%\n" +
            $"Crit Chance +{BonusStats.CritChance}% | Crit Damage +{BonusStats.CritDamage}%\n" +
            $"Armor Pen +{BonusStats.ArmorPenetration}% | Stamina Regen +{BonusStats.StaminaRegen}%"
        );
    }
}