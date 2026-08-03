using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltipAutoTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string title;
    private string description;
    private Sprite icon;

    public void SetData(string title, string description, Sprite icon = null)
    {
        this.title = title;
        this.description = description;
        this.icon = icon;
    }

    public void SetSkillData(CharacterSkillData skillData)
    {
        if (skillData == null) return;
        this.title = skillData.skillName;
        this.description = skillData.skillDescription; // Đã sửa tên biến thành skillDescription
        this.icon = skillData.skillIcon;
    }

    public void SetShopData(ShopItemSO shopItem)
    {
        if (shopItem == null) return;
        this.title = shopItem.itemName;
        this.description = shopItem.itemDescription;
        this.icon = shopItem.itemIcon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance != null && !string.IsNullOrEmpty(title))
        {
            UITooltipPanel.Instance.ShowTooltip(title, description, icon);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }
}