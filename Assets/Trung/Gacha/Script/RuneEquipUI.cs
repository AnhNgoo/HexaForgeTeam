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
    }

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

        slotImage.sprite = null;

        Color color =
            slotImage.color;

        color.a = 0f;

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
}