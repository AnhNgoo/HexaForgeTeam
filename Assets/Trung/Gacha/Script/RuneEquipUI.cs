using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class RuneEquipUI : MonoBehaviour
{
    [Header("Equip Slots")]
    [SerializeField] private Image slot1Image;

    [SerializeField] private Image slot2Image;

    [SerializeField] private Image slot3Image;

    [Header("Stat UI")]
    [SerializeField] private TMP_Text totalStatText;

    [Header("Empty Sprite")]
    [SerializeField] private Sprite emptySprite;

    [Header("Red Shape")]
    [SerializeField] private Sprite redCommonSprite;

    [SerializeField] private Sprite redRareSprite;

    [SerializeField] private Sprite redEpicSprite;

    [SerializeField] private Sprite redLegendarySprite;

    [Header("Green Shape")]
    [SerializeField] private Sprite greenCommonSprite;

    [SerializeField] private Sprite greenRareSprite;

    [SerializeField] private Sprite greenEpicSprite;

    [SerializeField] private Sprite greenLegendarySprite;

    [Header("Blue Shape")]
    [SerializeField] private Sprite blueCommonSprite;

    [SerializeField] private Sprite blueRareSprite;

    [SerializeField] private Sprite blueEpicSprite;

    [SerializeField] private Sprite blueLegendarySprite;

    private void Update()
    {
        RefreshEquipUI();
    }

    public void RefreshEquipUI()
    {
        ResetSlots();

        if (RuneInventory.Instance == null)
        {
            return;
        }

        for (int i = 0;
            i < RuneInventory.Instance.runes.Count;
            i++)
        {
            RuneData rune =
                RuneInventory.Instance.runes[i];

            if (!rune.isEquipped)
            {
                continue;
            }

            Image targetSlot =
                GetSlotImage(
                    rune.equippedSlotIndex);

            if (targetSlot == null)
            {
                continue;
            }

            targetSlot.sprite =
                GetRuneSprite(
                    rune);

            targetSlot.color =
                Color.white;
        }

        RefreshTotalStatText();
    }

    #region Total Stats

    private void RefreshTotalStatText()
    {
        if (totalStatText == null)
        {
            return;
        }

        if (RuneInventory.Instance == null)
        {
            return;
        }

        Dictionary<RuneStatType, float>
            totalStats =
            RuneInventory.Instance
            .GetStats();

        StringBuilder builder =
            new StringBuilder();

        foreach (var stat in totalStats)
{
    bool isPercent =
        IsPercentStat(stat.Key);

    float rawValue =
        stat.Value;

    float cap =
        RuneInventory.Instance
        .GetHardCap(stat.Key);

    bool reachedCap =
        rawValue >= cap;

    string color =
        reachedCap ?
        "#FF4C4C" :
        "#FFD966";

    float displayValue =
        Mathf.Min(
            rawValue,
            cap);

    if (isPercent)
    {
        builder.AppendLine(
            $"<color={color}>" +
            $"{GetStatName(stat.Key)} " +
            $"+{displayValue:F1}% " +
            $"/ {cap:F0}%</color>");
    }
    else
    {
        builder.AppendLine(
            $"<color={color}>" +
            $"{GetStatName(stat.Key)} " +
            $"+{displayValue:F0} " +
            $"/ {cap:F0}</color>");
    }
}
        totalStatText.text =
            builder.ToString();
    }

    #endregion

    #region Slot

    private void ResetSlots()
    {
        ResetSlot(slot1Image);

        ResetSlot(slot2Image);

        ResetSlot(slot3Image);
    }

    private void ResetSlot(
        Image slotImage)
    {
        if (slotImage == null)
        {
            return;
        }

        slotImage.sprite =
            emptySprite;

        Color color =
            slotImage.color;

        color.a = 1f;

        slotImage.color =
            color;
    }

    private Image GetSlotImage(
        int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
                return slot1Image;

            case 1:
                return slot2Image;

            case 2:
                return slot3Image;
        }

        return null;
    }

    #endregion

    #region Unequip

    public void UnequipBySlot(
        int slotIndex)
    {
        if (RuneInventory.Instance == null)
        {
            return;
        }

        for (int i = 0;
            i < RuneInventory.Instance.runes.Count;
            i++)
        {
            RuneData rune =
                RuneInventory.Instance.runes[i];

            if (!rune.isEquipped)
            {
                continue;
            }

            if (rune.equippedSlotIndex != slotIndex)
            {
                continue;
            }

            RuneInventory.Instance
                .UnequipRune(rune);

            RefreshEquipUI();

            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance
                    .RefreshInventory();
            }

            return;
        }
    }

    #endregion

    #region Rune Sprite

    private Sprite GetRuneSprite(
        RuneData rune)
    {
        switch (rune.runeColor)
        {
            case RuneColor.Red:

                return GetRedSprite(
                    rune.runeRarity);

            case RuneColor.Green:

                return GetGreenSprite(
                    rune.runeRarity);

            case RuneColor.Blue:

                return GetBlueSprite(
                    rune.runeRarity);
        }

        return null;
    }

    #endregion

    #region Red

    private Sprite GetRedSprite(
        RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common:
                return redCommonSprite;

            case RuneRarity.Rare:
                return redRareSprite;

            case RuneRarity.Epic:
                return redEpicSprite;

            case RuneRarity.Legendary:
                return redLegendarySprite;
        }

        return null;
    }

    #endregion

    #region Green

    private Sprite GetGreenSprite(
        RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common:
                return greenCommonSprite;

            case RuneRarity.Rare:
                return greenRareSprite;

            case RuneRarity.Epic:
                return greenEpicSprite;

            case RuneRarity.Legendary:
                return greenLegendarySprite;
        }

        return null;
    }

    #endregion

    #region Blue

    private Sprite GetBlueSprite(
        RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common:
                return blueCommonSprite;

            case RuneRarity.Rare:
                return blueRareSprite;

            case RuneRarity.Epic:
                return blueEpicSprite;

            case RuneRarity.Legendary:
                return blueLegendarySprite;
        }

        return null;
    }

    #endregion

    #region Stat Helper

    private bool IsPercentStat(
        RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HPPercent:
            case RuneStatType.MPPercent:
            case RuneStatType.StaminaPercent:

            case RuneStatType.ATKPercent:
            case RuneStatType.MATKPercent:
            case RuneStatType.DEFPercent:

            case RuneStatType.AttackSpeed:
            case RuneStatType.CritChance:
            case RuneStatType.CritDamage:
            case RuneStatType.ArmorPenetration:
            case RuneStatType.StaminaRegen:

                return true;
        }

        return false;
    }

    private string GetStatName(
        RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP:
                return "HP";

            case RuneStatType.HPPercent:
                return "HP";

            case RuneStatType.MP:
                return "MP";

            case RuneStatType.MPPercent:
                return "MP";

            case RuneStatType.Stamina:
                return "Stamina";

            case RuneStatType.StaminaPercent:
                return "Stamina";

            case RuneStatType.ATK:
                return "ATK";

            case RuneStatType.ATKPercent:
                return "ATK";

            case RuneStatType.MATK:
                return "MATK";

            case RuneStatType.MATKPercent:
                return "MATK";

            case RuneStatType.DEF:
                return "DEF";

            case RuneStatType.DEFPercent:
                return "DEF";

            case RuneStatType.AttackSpeed:
                return "Attack Speed";

            case RuneStatType.CritChance:
                return "Crit Chance";

            case RuneStatType.CritDamage:
                return "Crit Damage";

            case RuneStatType.ArmorPenetration:
                return "Armor Penetration";

            case RuneStatType.StaminaRegen:
                return "Stamina Regen";
        }

        return "Unknown";
    }

    #endregion

}