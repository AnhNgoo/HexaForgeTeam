using System.Collections.Generic;
using UnityEngine;

public class LobbyStatManager : MonoBehaviour
{
    public static LobbyStatManager Instance;

    [Header("Current Evaluated Stats")]
    public CombinedLobbyStats currentCombinedStats = new CombinedLobbyStats();

    [Header("Exposed Converted Stats for Character")]
    public CharacterStats currentRuneAndAccountStats = new CharacterStats();

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

        EventManager.Subscribe(GameEvent.OnPlayerSpawned, OnPlayerSpawnedHandler);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnPlayerSpawned, OnPlayerSpawnedHandler);
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

        ApplyStatsToCurrentSpawnedPlayer();
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
                case RuneStatType.MPRegen: runeData.MPRegen += stat.Value; break;
                case RuneStatType.Stamina: runeData.Stamina += stat.Value; break;
                case RuneStatType.StaminaPercent: runeData.StaminaPercent += stat.Value; break;
                case RuneStatType.StaminaRegen: runeData.StaminaRegen += stat.Value; break;
                case RuneStatType.ATK: runeData.ATK += stat.Value; break;
                case RuneStatType.ATKPercent: runeData.ATKPercent += stat.Value; break;
                case RuneStatType.DEF: runeData.DEF += stat.Value; break;
                case RuneStatType.DEFPercent: runeData.DEFPercent += stat.Value; break;
                case RuneStatType.Speed: runeData.Speed += stat.Value; break;
                case RuneStatType.PoisonDamage: runeData.PoisonDamage += stat.Value; break;
                case RuneStatType.AllStats:
                    runeData.HP += stat.Value;
                    runeData.MP += stat.Value;
                    runeData.MPRegen += stat.Value;
                    runeData.Stamina += stat.Value;
                    runeData.StaminaRegen += stat.Value;
                    runeData.ATK += stat.Value;
                    runeData.DEF += stat.Value;
                    runeData.Speed += stat.Value;
                    runeData.PoisonDamage += stat.Value;
                    break;
            }
        }
    }

    private void OnPlayerSpawnedHandler(object playerTransformObj)
    {
        Transform playerTransform = playerTransformObj as Transform;
        if (playerTransform == null) return;

        CharacterBase charBase = playerTransform.GetComponent<CharacterBase>();
        if (charBase == null) return;

        ApplyStatsToCharacter(charBase);
    }

    public void ApplyStatsToCurrentSpawnedPlayer()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.CurrentCharacterBase != null)
        {
            ApplyStatsToCharacter(PlayerManager.Instance.CurrentCharacterBase);
        }
    }

    public void ApplyStatsToCharacter(CharacterBase characterBase)
    {
        if (characterBase == null || characterBase.CharacterStat == null) return;

        CharacterType deployedType = CharacterType.Kael;
        if (CharacterManager.Instance != null)
        {
            deployedType = CharacterManager.Instance.GetSelectedCharacter();
        }

        currentCombinedStats.targetCharacter = deployedType;
        currentCombinedStats.levelBonusStats.Reset();
        currentCombinedStats.runeBonusStats.Reset();

        CalculateAccountLevelStats();
        CalculateRuneStats(deployedType);

        // Gộp toàn bộ Rune Stats và Account Level Stats (HP, ATK vĩnh viễn) thành một gói duy nhất
        currentRuneAndAccountStats = GetCalculatedCharacterStats(deployedType, characterBase.CharacterStat.OriginStats);

        // Truyền trực tiếp vào SetRuneStats của Player (Không đụng vào SetLevelStats của Run Gameplay)
        characterBase.CharacterStat.SetRuneStats(currentRuneAndAccountStats);

        Debug.Log($"<color=#00FFCC><b>[LobbyStatManager]</b> Đã nạp Rune & Account Level Stats cho nhân vật: {deployedType}!</color>");
    }

    public CharacterStats GetCalculatedCharacterStats(CharacterType charType, CharacterStats originStats)
    {
        CharacterStats stats = new CharacterStats();

        float baseHp = originStats != null ? originStats.maxHealth : 0f;
        float baseMp = originStats != null ? originStats.mp : 0f;
        float baseSta = originStats != null ? originStats.stamina : 0f;
        float baseAtk = originStats != null ? originStats.damage : 0f;
        float baseDef = originStats != null ? originStats.defense : 0f;

        LobbyStatData runeData = currentCombinedStats.runeBonusStats;
        LobbyStatData accountLevelData = currentCombinedStats.levelBonusStats;

        stats.maxHealth = runeData.HP + accountLevelData.HP + (baseHp * (runeData.HPPercent / 100f));
        stats.damage = runeData.ATK + accountLevelData.ATK + (baseAtk * (runeData.ATKPercent / 100f));
        stats.defense = runeData.DEF + (baseDef * (runeData.DEFPercent / 100f));
        stats.mp = runeData.MP + (baseMp * (runeData.MPPercent / 100f));
        stats.mpRegen = runeData.MPRegen;
        stats.stamina = runeData.Stamina + (baseSta * (runeData.StaminaPercent / 100f));
        stats.staminaRegen = runeData.StaminaRegen;
        stats.speed = runeData.Speed;
        stats.poisonDamage = runeData.PoisonDamage;

        return stats;
    }

    public CombinedLobbyStats GetCombinedStats() => currentCombinedStats;

    private void DebugBonusStats()
    {
        LobbyStatData lv = currentCombinedStats.levelBonusStats;
        LobbyStatData rune = currentCombinedStats.runeBonusStats;

        Debug.Log(
            $"===== LOBBY BONUS [{currentCombinedStats.targetCharacter}] =====\n" +
            $"[ACCOUNT LEVEL] HP +{lv.HP} | ATK +{lv.ATK}\n" +
            $"[RUNE BONUS] HP +{rune.HP} ({rune.HPPercent}%) | MP +{rune.MP} ({rune.MPPercent}%) | STA +{rune.Stamina} ({rune.StaminaPercent}%)\n" +
            $"[COMBAT STATS] ATK +{rune.ATK} ({rune.ATKPercent}%) | DEF +{rune.DEF} ({rune.DEFPercent}%) | SPD +{rune.Speed} | POISON +{rune.PoisonDamage}"
        );
    }
}