using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance;

    [Header("Rarity Rate Config (GDD Standard)")]
    [SerializeField] [Range(0, 100)] private int commonRate = 65;
    [SerializeField] [Range(0, 100)] private int rareRate = 25;
    [SerializeField] [Range(0, 100)] private int epicRate = 8;
    [SerializeField] [Range(0, 100)] private int legendaryRate = 2;

    [Header("Cost Config Per Roll")]
    [SerializeField] private int gemCostPerRoll = 120;
    [SerializeField] private string ticketItemID = "GACHA_TICKET_01";

    [Header("Inventory Protection Config")]
    [SerializeField] private int maxInventorySlots = 100;

    private int lastRollCost;
    private int lastRollAmount;
    private int revealedCardCount;
    private int totalCardCount;
    private bool isRollActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool IsRollActive() => isRollActive;

    public void Roll1() => Roll(1);
    public void Roll10() => Roll(10);

    private void Roll(int amount)
    {
        if (isRollActive) return;

        if (RuneInventoryManager.Instance != null)
        {
            int currentRuneCount = RuneInventoryManager.Instance.runes.Count;
            if (currentRuneCount + amount > maxInventorySlots)
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("Inventory is full! Please dismantle some runes.", Color.red);
                }
                return;
            }
        }

        int ownedTickets = 0;
        if (InventoryItemManager.Instance != null)
        {
            ownedTickets = InventoryItemManager.Instance.GetItemQuantity(ticketItemID);
        }

        int ticketsToUse = Mathf.Min(ownedTickets, amount);
        int missingRolls = amount - ticketsToUse;
        
        int requiredGem = 0;
        if (missingRolls > 0)
        {
            if (amount == 10 && ticketsToUse == 0)
            {
                requiredGem = 1080;
            }
            else
            {
                requiredGem = missingRolls * gemCostPerRoll;
            }
        }

        if (missingRolls > 0 && (GemManager.Instance == null || GemManager.Instance.GetCurrentGem() < requiredGem))
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Not enough Tickets or Gems to perform gacha roll!", Color.red);
            }
            return;
        }

        if (ticketsToUse > 0 && InventoryItemManager.Instance != null)
        {
            InventoryItemManager.Instance.SpendItem(ticketItemID, ticketsToUse);
        }

        if (requiredGem > 0 && GemManager.Instance != null)
        {
            GemManager.Instance.SpendGem(requiredGem);
        }

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.RefreshCurrencyUI();
        }

        lastRollCost = requiredGem;
        lastRollAmount = amount;

        if (GachaUI.Instance != null)
        {
            GachaUI.Instance.ClearCards();
            GachaUI.Instance.SetMainRollButtonsInteractable(false);
            GachaUI.Instance.ToggleUIPanels(true);
            GachaUI.Instance.SetSkipButtonActive(true);
        }

        revealedCardCount = 0;
        totalCardCount = amount;
        isRollActive = true;

        RuneRarity highestRarityInThisRoll = RuneRarity.Common;
        List<RuneData> rolledRunesData = new List<RuneData>();

        for (int i = 0; i < amount; i++)
        {
            RuneData rune = GenerateRandomRune();
            rolledRunesData.Add(rune);

            if (rune.runeRarity > highestRarityInThisRoll)
            {
                highestRarityInThisRoll = rune.runeRarity;
            }

            if (RuneInventoryManager.Instance != null) RuneInventoryManager.Instance.AddRune(rune);

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.AddRollProgress(1);
                if (rune.runeRarity == RuneRarity.Legendary) AchievementManager.Instance.AddLegendaryProgress(1);
            }
        }

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }

        if (GachaUI.Instance != null)
        {
            GachaUI.Instance.PlaySummoningFX(highestRarityInThisRoll, rolledRunesData);
        }
    }

    public void TriggerLegendaryRevealAction(RuneCardUI legendaryCard)
    {
        if (!isRollActive) return;

        if (GachaUI.Instance != null)
        {
            GachaUI.Instance.TriggerLegendaryRevealAction(legendaryCard);
        }
    }

    public void SkipAllGachaAnimations()
    {
        if (!isRollActive) return;

        if (GachaUI.Instance != null)
        {
            GachaUI.Instance.StopAllSummoningCoroutines();
            GachaUI.Instance.ForceInstantRevealAll();
            GachaUI.Instance.ToggleUIPanels(false);
            GachaUI.Instance.SetSkipButtonActive(false);
            GachaUI.Instance.RefreshCostUI();
        }

        revealedCardCount = totalCardCount;
        isRollActive = false;

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify("Animations skipped. Runes added to vault.", Color.white);
        }
    }

    public void NotifyCardRevealed()
    {
        if (!isRollActive) return;

        revealedCardCount++;
        if (revealedCardCount < totalCardCount) return;

        isRollActive = false;

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(true);
            LobbyHUDTopBar.Instance.RefreshCurrencyUI();
        }

        if (GachaUI.Instance != null)
        {
            GachaUI.Instance.ToggleUIPanels(false);
            GachaUI.Instance.SetSkipButtonActive(false);
            GachaUI.Instance.RefreshCostUI();
        }

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify("All runes successfully summoned!", Color.green);
        }
    }

    public void CloseResultPanel()
    {
        isRollActive = false;

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(true);
            LobbyHUDTopBar.Instance.RefreshCurrencyUI();
        }

        if (GachaUI.Instance != null)
        {
            GachaUI.Instance.SetResultPanelActive(false);
            GachaUI.Instance.ClearCards();
            GachaUI.Instance.RefreshCostUI();
        }
    }
    public void ReRoll()
    {
        if (lastRollAmount <= 0) return;
        Roll(lastRollAmount);
    }

    private RuneData GenerateRandomRune()
    {
        RuneColor runeColor = RandomRuneColor();
        RuneRarity runeRarity = RandomRuneRarity();
        RuneData rune = new RuneData(runeColor, runeRarity);
        AssignRuneLore(rune);
        GenerateAffixes(rune);
        return rune;
    }

    private RuneColor RandomRuneColor()
    {
        int random = Random.Range(0, 100);
        if (random < 33) return RuneColor.Red;
        if (random < 66) return RuneColor.Green;
        return RuneColor.Blue;
    }

    private RuneRarity RandomRuneRarity()
    {
        int totalRate = commonRate + rareRate + epicRate + legendaryRate;
        if (totalRate <= 0) return RuneRarity.Common;

        int random = Random.Range(0, totalRate);
        if (random < commonRate) return RuneRarity.Common;
        random -= commonRate;
        if (random < rareRate) return RuneRarity.Rare;
        random -= rareRate;
        if (random < epicRate) return RuneRarity.Epic;
        return RuneRarity.Legendary;
    }

    private void GenerateAffixes(RuneData rune)
    {
        int affixCount = GetAffixCount(rune.runeRarity);
        List<RuneStatType> usedStats = new List<RuneStatType>();

        for (int i = 0; i < affixCount; i++)
        {
            RuneStatType statType = GetRandomStat(usedStats);
            usedStats.Add(statType);
            float value = GetRandomValue(statType, rune.runeRarity);
            rune.affixes.Add(new RuneAffixData(statType, value));
        }
    }

    private int GetAffixCount(RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common: return 1;
            case RuneRarity.Rare: return 2;
            case RuneRarity.Epic: return 3;
            case RuneRarity.Legendary: return 4;
        }
        return 1;
    }

    private RuneStatType GetRandomStat(List<RuneStatType> usedStats)
    {
        List<RuneStatType> pool = new List<RuneStatType>()
        {
            RuneStatType.HP, RuneStatType.HPPercent, RuneStatType.MP, RuneStatType.MPPercent,
            RuneStatType.Stamina, RuneStatType.StaminaPercent, RuneStatType.ATK, RuneStatType.ATKPercent,
            RuneStatType.DEF, RuneStatType.DEFPercent, RuneStatType.CritChance, RuneStatType.CritDamage,
            RuneStatType.ArmorPenetration, RuneStatType.StaminaRegen
        };

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (usedStats.Contains(pool[i])) pool.RemoveAt(i);
        }
        return pool[Random.Range(0, pool.Count)];
    }

    private float GetRandomValue(RuneStatType statType, RuneRarity rarity)
    {
        switch (statType)
        {
            case RuneStatType.HP: return GetValueByRarity(rarity, 80f, 180f, 180f, 350f, 350f, 650f, 650f, 1200f);
            case RuneStatType.MP: return GetValueByRarity(rarity, 25f, 60f, 60f, 120f, 120f, 220f, 220f, 400f);
            case RuneStatType.Stamina: return GetValueByRarity(rarity, 15f, 40f, 40f, 80f, 80f, 140f, 140f, 250f);
            case RuneStatType.ATK: return GetValueByRarity(rarity, 3f, 8f, 8f, 18f, 18f, 35f, 35f, 60f);
            case RuneStatType.DEF: return GetValueByRarity(rarity, 2f, 6f, 6f, 14f, 14f, 28f, 28f, 50f);
            case RuneStatType.HPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 20f);
            case RuneStatType.MPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.StaminaPercent: return GetValueByRarity(rarity, 3f, 5f, 5f, 9f, 9f, 15f, 15f, 25f);
            case RuneStatType.ATKPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.DEFPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.CritChance: return GetValueByRarity(rarity, 1f, 3f, 3f, 6f, 6f, 10f, 10f, 18f);
            case RuneStatType.CritDamage: return GetValueByRarity(rarity, 4f, 8f, 8f, 15f, 15f, 25f, 25f, 40f);
            case RuneStatType.ArmorPenetration: return GetValueByRarity(rarity, 2f, 5f, 5f, 9f, 9f, 15f, 15f, 25f);
            case RuneStatType.StaminaRegen: return GetValueByRarity(rarity, 3f, 6f, 6f, 10f, 10f, 18f, 18f, 30f);
        }
        return 1f;
    }

    private float GetValueByRarity(RuneRarity rarity, float cMin, float cMax, float rMin, float rMax, float eMin, float eMax, float lMin, float lMax)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return Random.Range(cMin, cMax);
            case RuneRarity.Rare: return Random.Range(rMin, rMax);
            case RuneRarity.Epic: return Random.Range(eMin, eMax);
            case RuneRarity.Legendary: return Random.Range(lMin, lMax);
        }
        return 1f;
    }

    private void AssignRuneLore(RuneData rune)
    {
        switch (rune.runeColor)
        {
            case RuneColor.Red: AssignRedLore(rune); break;
            case RuneColor.Green: AssignGreenLore(rune); break;
            case RuneColor.Blue: AssignBlueLore(rune); break;
        }
    }

    private void AssignRedLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Ashfang"; rune.runeLore = "Its heat faded long ago, yet the scar remains."; break;
            case RuneRarity.Rare: rune.runeName = "Blood Oath"; rune.runeLore = "The knight survived the battle. His comrades did not."; break;
            case RuneRarity.Epic: rune.runeName = "Heart of Ruin"; rune.runeLore = "Every beat echoed like a war drum beneath the earth."; break;
            case RuneRarity.Legendary: rune.runeName = "Crimson Crown"; rune.runeLore = "Kings burned kingdoms to wear it for a single night."; break;
        }
    }

    private void AssignGreenLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Wiltroot"; rune.runeLore = "It grew where no light should ever reach."; break;
            case RuneRarity.Rare: rune.runeName = "Verdant Pulse"; rune.runeLore = "The forest whispered back when spoken to."; break;
            case RuneRarity.Epic: rune.runeName = "Hollow Bloom"; rune.runeLore = "Flowers fed on the dead beneath the ruins."; break;
            case RuneRarity.Legendary: rune.runeName = "Worldsap Core"; rune.runeLore = "Its roots once held an entire civilization together."; break;
        }
    }

    private void AssignBlueLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Frost Vein"; rune.runeLore = "Cold enough to silence fear itself."; break;
            case RuneRarity.Rare: rune.runeName = "Moon Shard"; rune.runeLore = "Fragments of a sky long forgotten."; break;
            case RuneRarity.Epic: rune.runeName = "Deep Current"; rune.runeLore = "Something ancient moved beneath the tide."; break;
            case RuneRarity.Legendary: rune.runeName = "Eye of Eternity"; rune.runeLore = "It watched the end before time understood death."; break;
        }
    }
}