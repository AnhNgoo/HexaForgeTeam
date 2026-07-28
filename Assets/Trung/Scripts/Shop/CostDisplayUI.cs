using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CostDisplayUI : MonoBehaviour
{
    [System.Serializable]
    public class CostSlot
    {
        public GameObject rootGroup;
        public Image iconImage;
        public TMP_Text amountText;
    }

    [Header("Cost Slots Configuration (Tối đa 3 slots)")]
    [SerializeField] private List<CostSlot> costSlots = new List<CostSlot>();

    [Header("Plus Signs (Dấu cộng xen kẽ giữa các ô)")]
    [SerializeField] private List<GameObject> plusSigns = new List<GameObject>();

    /// <summary>
    /// Hàm chính để cập nhật danh sách Cost / Phần thưởng lên Prefab
    /// </summary>
    public void SetupCost(List<CostData> costs)
    {
        HideAllSlots();

        if (costs == null || costs.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        int validCostCount = 0;

        for (int i = 0; i < costs.Count && i < costSlots.Count; i++)
        {
            CostData cost = costs[i];
            if (cost == null || string.IsNullOrEmpty(cost.itemID) || cost.amount <= 0) continue;

            CostSlot slot = costSlots[i];
            if (slot.rootGroup != null) slot.rootGroup.SetActive(true);

            // Gán số lượng
            if (slot.amountText != null)
            {
                slot.amountText.SetTextSafe(cost.amount.ToString("N0"));
            }

            // Gán Icon: Thử tìm trong InventoryItemDatabase
            if (slot.iconImage != null)
            {
                Sprite iconSprite = GetSpriteFromDatabases(cost.itemID);

                if (iconSprite != null)
                {
                    slot.iconImage.sprite = iconSprite;
                    slot.iconImage.gameObject.SetActive(true);
                }
                else
                {
                    // Nếu không có Sprite Icon (ví dụ là mở khóa Nhân vật), ẩn riêng phần Image icon đi để chỉ hiện số/tên
                    slot.iconImage.gameObject.SetActive(false);
                }
            }

            // Bật dấu '+' phía trước slot này nếu là slot thứ 2 trở đi
            if (validCostCount > 0 && (validCostCount - 1) < plusSigns.Count)
            {
                if (plusSigns[validCostCount - 1] != null)
                {
                    plusSigns[validCostCount - 1].SetActive(true);
                }
            }

            validCostCount++;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private Sprite GetSpriteFromDatabases(string id)
    {
        // 1. Thử tìm Sprite từ InventoryItemDatabase
        if (InventoryItemDatabase.Instance != null)
        {
            Sprite itemSprite = InventoryItemDatabase.Instance.GetItemSprite(id);
            if (itemSprite != null) return itemSprite;
        }

        return null;
    }

    private void HideAllSlots()
    {
        for (int i = 0; i < costSlots.Count; i++)
        {
            if (costSlots[i].rootGroup != null) costSlots[i].rootGroup.SetActive(false);
        }

        for (int i = 0; i < plusSigns.Count; i++)
        {
            if (plusSigns[i] != null) plusSigns[i].SetActive(false);
        }
    }
}