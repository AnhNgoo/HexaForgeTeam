using UnityEngine;
using UnityEngine.EventSystems;

public class RuneEquipSlotDropHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int slotIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (RuneEquipUI.Instance == null || RuneInventoryManager.Instance == null) return;

        CharacterType currentType = RuneEquipUI.Instance.GetViewingCharacter();
        var build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (build != null && slotIndex < build.equippedRuneIDs.Length)
        {
            string equippedID = build.equippedRuneIDs[slotIndex];
            if (!string.IsNullOrEmpty(equippedID))
            {
                RuneData equippedRune = RuneInventoryManager.Instance.runes.Find(r => r.runeID == equippedID);
                if (equippedRune != null)
                {
                    if (RuneDetailInfoPanel.Instance != null)
                    {
                        RuneDetailInfoPanel.Instance.DisplayRuneInfo(equippedRune);
                    }

                    if (UITooltipPanel.Instance != null)
                    {
                        string title = $"<color={GetRarityHexColor(equippedRune.runeRarity)}>{equippedRune.runeName.ToUpper()}</color>";
                        string details = $"<b>Rarity:</b> {equippedRune.runeRarity} | <b>Element:</b> {equippedRune.runeColor}\n\n";

                        for (int i = 0; i < equippedRune.affixes.Count; i++)
                        {
                            var affix = equippedRune.affixes[i];
                            string sign = affix.value >= 0 ? "+" : "";
                            details += $"- {affix.statType}: <color=#00FFCC>{sign}{affix.value:F1}</color>\n";
                        }

                        if (!string.IsNullOrEmpty(equippedRune.runeLore))
                        {
                            details += $"\n<i>\"{equippedRune.runeLore}\"</i>";
                        }

                        UITooltipPanel.Instance.ShowTooltip(title, details);
                    }
                }
            }
            else
            {
                if (UITooltipPanel.Instance != null)
                {
                    RuneColor reqColor = RuneEquipUI.Instance.GetSlotRequiredColor(currentType, slotIndex);
                    UITooltipPanel.Instance.ShowTooltip($"Empty Slot {slotIndex + 1}", $"Equip a <color={(reqColor == RuneColor.Red ? "red" : reqColor == RuneColor.Green ? "green" : "cyan")}>{reqColor}</color> Rune here.");
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        RuneCardUI draggedCard = eventData.pointerDrag.GetComponent<RuneCardUI>();
        if (draggedCard == null || draggedCard.GetRuneData() == null) return;

        RuneData runeData = draggedCard.GetRuneData();

        if (RuneEquipUI.Instance == null || RuneInventoryManager.Instance == null) return;

        CharacterType currentType = RuneEquipUI.Instance.GetViewingCharacter();

        bool isEquippedByOtherChar = false;
        string ownerName = "";
        if (CharacterManager.Instance != null)
        {
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                if (charType == currentType) continue;

                var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                if (build != null && build.equippedRuneIDs != null)
                {
                    for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                    {
                        if (build.equippedRuneIDs[i] == runeData.runeID)
                        {
                            isEquippedByOtherChar = true;
                            ownerName = charType.ToString().ToUpper();
                            break;
                        }
                    }
                }
            }
        }

        if (isEquippedByOtherChar)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Rune is currently equipped by {ownerName}!", Color.red);
            }
            return;
        }

        RuneColor requiredColor = RuneEquipUI.Instance.GetSlotRequiredColor(currentType, slotIndex);

        bool isOrigin = false;
        if (runeData.affixes != null)
        {
            for (int i = 0; i < runeData.affixes.Count; i++)
            {
                if (runeData.affixes[i].statType == RuneStatType.AllStats)
                {
                    isOrigin = true;
                    break;
                }
            }
        }

        if (!isOrigin && runeData.runeColor != requiredColor)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Slot {slotIndex + 1} requires a {requiredColor} Rune!", Color.yellow);
            }
            return;
        }

        bool equipped = RuneInventoryManager.Instance.EquipRune(runeData, currentType);
        if (equipped)
        {
            RuneEquipUI.Instance.RefreshEquipUI();
            if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
            if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Equipped {runeData.runeName} into Slot {slotIndex + 1}!", Color.green);
            }
        }
    }

    private string GetRarityHexColor(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return "#FFFFFF";
            case RuneRarity.Rare: return "#3399FF";
            case RuneRarity.Epic: return "#B266FF";
            case RuneRarity.Legendary: return "#FF9900";
        }
        return "#FFFFFF";
    }
}