using System.Collections.Generic;
using UnityEngine;

public class LobbyStatManager : MonoBehaviour
{
    public static LobbyStatManager Instance;

    [Header("Current Evaluated Stats")]
    public CombinedLobbyStats currentCombinedStats = new CombinedLobbyStats();

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
        CharacterType targetChar = CharacterType.Kael;

        if (RuneEquipUI.Instance != null && RuneEquipUI.Instance.gameObject.activeInHierarchy)
        {
            targetChar = RuneEquipUI.Instance.GetViewingCharacter();
        }
        else if (CharacterManager.Instance != null)
        {
            targetChar = CharacterManager.Instance.GetSelectedCharacter();
        }

        currentCombinedStats.targetCharacter = targetChar;
        currentCombinedStats.levelBonusStats.Reset();
        currentCombinedStats.runeBonusStats.Reset();

        CalculateAccountLevelStats();
        CalculateRuneStats(targetChar);

        DebugBonusStats();
    }

    private void CalculateAccountLevelStats()
    {
        if (AccountLevelManager.Instance == null) return;

        currentCombinedStats.levelBonusStats.HP += AccountLevelManager.Instance.GetHPBonus();
        currentCombinedStats.levelBonusStats.ATK += AccountLevelManager.Instance.GetATKBonus();
    }

    private void CalculateRuneStats(CharacterType targetChar)
    {
        if (RuneInventoryManager.Instance == null) return;

        Dictionary<RuneStatType, float> stats = RuneInventoryManager.Instance.GetStats(targetChar);
        LobbyStatData runeData = currentCombinedStats.runeBonusStats;

        foreach (var stat in stats)
        {
            switch (stat.Key)
            {
                case RuneStatType.HP: runeData.HP += stat.Value; break;
                case RuneStatType.HPPercent: runeData.HPPercent += stat.Value; break;
                case RuneStatType.MP: runeData.MP += stat.Value; break;
                case RuneStatType.MPPercent: runeData.MPPercent += stat.Value; break;
                case RuneStatType.Stamina: runeData.Stamina += stat.Value; break;
                case RuneStatType.StaminaPercent: runeData.StaminaPercent += stat.Value; break;
                case RuneStatType.ATK: runeData.ATK += stat.Value; break;
                case RuneStatType.ATKPercent: runeData.ATKPercent += stat.Value; break;
                case RuneStatType.DEF: runeData.DEF += stat.Value; break;
                case RuneStatType.DEFPercent: runeData.DEFPercent += stat.Value; break;
                case RuneStatType.CritChance: runeData.CritChance += stat.Value; break;
                case RuneStatType.CritDamage: runeData.CritDamage += stat.Value; break;
                case RuneStatType.ArmorPenetration: runeData.ArmorPenetration += stat.Value; break;
                case RuneStatType.StaminaRegen: runeData.StaminaRegen += stat.Value; break;
                case RuneStatType.AllStats:
                    runeData.HP += stat.Value;
                    runeData.MP += stat.Value;
                    runeData.Stamina += stat.Value;
                    runeData.ATK += stat.Value;
                    runeData.DEF += stat.Value;
                    runeData.CritChance += stat.Value;
                    runeData.CritDamage += stat.Value;
                    runeData.ArmorPenetration += stat.Value;
                    runeData.StaminaRegen += stat.Value;
                    break;
            }
        }
    }

    public CombinedLobbyStats GetCombinedStats() => currentCombinedStats;

    private void DebugBonusStats()
    {
        LobbyStatData lv = currentCombinedStats.levelBonusStats;
        LobbyStatData rune = currentCombinedStats.runeBonusStats;

        Debug.Log(
            $"===== LOBBY BONUS [{currentCombinedStats.targetCharacter}] =====\n" +
            $"[LEVEL BONUS] HP +{lv.HP} | ATK +{lv.ATK}\n" +
            $"[RUNE BONUS] HP +{rune.HP} ({rune.HPPercent}%) | ATK +{rune.ATK} ({rune.ATKPercent}%) | DEF +{rune.DEF} ({rune.DEFPercent}%)\n" +
            $"[RUNE COMBAT] Crit Chance +{rune.CritChance}% | Crit DMG +{rune.CritDamage}% | Armor Pen +{rune.ArmorPenetration}%"
        );
    }
}