using UnityEngine;
using UnityEngine.EventSystems;

public class RuneEquipSlotDropHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
    [SerializeField] private int slotIndex; // 0 cho Slot 1, 1 cho Slot 2, 2 cho Slot 3

    // Khi con trỏ chuột rê qua Ô Slot trang bị -> Cập nhật thông tin viên ngọc đang gắn trên ô này (nếu có)
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
                if (equippedRune != null && RuneDetailInfoPanel.Instance != null)
                {
                    RuneDetailInfoPanel.Instance.DisplayRuneInfo(equippedRune);
                }
            }
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

        // 1. Kiểm tra xem ngọc có đang bị nhân vật KHÁC trang bị hay không
        bool isEquippedByOtherChar = false;
        string ownerName = "";
        if (CharacterManager.Instance != null)
        {
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                if (charType == currentType) continue; // Bỏ qua nhân vật hiện tại

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

        // 2. Kiểm tra màu ngọc trùng khớp yêu cầu ô Slot
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

        // 3. Tiến hành gắn ngọc vào đúng Slot
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
}