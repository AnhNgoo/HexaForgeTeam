using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLevel : LoadComponents
{
    [SerializeField] private StatGainedLevelUp statGainedLevelUp;
    public StatGainedLevelUp StatGainedLevelUp => statGainedLevelUp;

    [SerializeField] private int currentLevel = 0;
    public int CurrentLevel => currentLevel;
    [SerializeField] private int maxLevel;
    public int MaxLevel => maxLevel;

    private CharacterBase characterBase;

    protected override void LoadComponent()
    {
        if (statGainedLevelUp == null)
            statGainedLevelUp = Resources.Load<StatGainedLevelUp>("ScriptableObjects/StatGainedLevelUp/StatGainedLevelUp");
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void Init(CharacterBase characterBase)
    {
        this.characterBase = characterBase;
        ResetLevel();
        maxLevel = statGainedLevelUp.maxLevel;
    }

    public void LevelUp()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            CharacterStats levelStats = CalculateLevelUpStats(currentLevel); // Tính toán các chỉ số tăng lên dựa trên cấp độ hiện tại
            characterBase.CharacterStat.SetLevelStats(levelStats);
            EventManager.Notify(GameEvent.OnUpdateLevel, currentLevel);
        }
        else
        {
            Debug.LogWarning("Đã đạt cấp tối đa!");
        }
    }

    public void DecreaseLevel(int amount = 1)
    {
        if (currentLevel > 0)
        {
            currentLevel -= amount;
            if (currentLevel < 0)
                currentLevel = 0;
            CharacterStats levelStats = CalculateLevelUpStats(currentLevel); // Tính toán các chỉ số giảm xuống dựa trên cấp độ hiện tại
            characterBase.CharacterStat.SetLevelStats(levelStats);
            EventManager.Notify(GameEvent.OnUpdateLevel, currentLevel);
        }
        else
        {
            Debug.LogWarning("Đã đạt cấp tối thiểu!");
        }
    }

    public void ResetLevel()
    {
        currentLevel = 0;
        characterBase.CharacterStat.SetLevelStats(new CharacterStats());
        EventManager.Notify(GameEvent.OnUpdateLevel, currentLevel);
    }

    private CharacterStats CalculateLevelUpStats(int level)
    {
        CharacterStats levelStats = new CharacterStats();
        CharacterStats baseStats = statGainedLevelUp != null ? statGainedLevelUp.characterStats : null;

        if (baseStats == null)
        {
            Debug.LogWarning("CharacterLevel: statGainedLevelUp.characterStats is not assigned.");
            return levelStats;
        }

        levelStats.damage = baseStats.damage * level;
        levelStats.defense = baseStats.defense * level;
        levelStats.maxHealth = baseStats.maxHealth * level;
        levelStats.speed = baseStats.speed * level;
        levelStats.stamina = baseStats.stamina * level;
        levelStats.staminaRegen = baseStats.staminaRegen * level;
        levelStats.mp = baseStats.mp * level;
        levelStats.mpRegen = baseStats.mpRegen * level;

        return levelStats;
    }
}
