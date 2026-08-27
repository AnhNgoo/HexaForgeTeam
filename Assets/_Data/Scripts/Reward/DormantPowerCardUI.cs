using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DormantPowerCardUI : LoadComponents
{
    [SerializeField] private Image background;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardTypeText;
    [SerializeField] private TMP_Text rewardNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Button cardButton;
    [SerializeField] private GameObject selectedHighlight;

    public BossRewardDataSO Reward => reward;

    private BossRewardDataSO reward;
    private UnityAction selectAction;

    public void Setup(BossRewardDataSO data, Action<BossRewardDataSO> onSelected)
    {
        reward = data;
        gameObject.SetActive(reward != null);

        if (reward == null)
            return;

        rewardIcon.sprite = reward.DisplayIcon;
        rewardIcon.enabled = reward.DisplayIcon != null;
        rewardTypeText.text = reward.TypeLabel;
        rewardNameText.text = reward.DisplayName;
        rarityText.text = reward.DisplayRarity.ToString();

        Color rarityColor = GetRarityColor(reward.DisplayRarity);
        rarityBorder.color = rarityColor;
        background.color = new Color(
            rarityColor.r * 0.28f,
            rarityColor.g * 0.28f,
            rarityColor.b * 0.28f,
            0.95f);

        if (selectAction != null)
            cardButton.onClick.RemoveListener(selectAction);

        selectAction = () => onSelected?.Invoke(reward);
        cardButton.onClick.AddListener(selectAction);
        cardButton.interactable = true;

        SetSelected(false);
    }

    public void SetSelected(bool value)
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(value);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetInteractable(bool value)
    {
        if (cardButton != null)
            cardButton.interactable = value;
    }
    protected override void LoadComponent()
    {
        if (background == null)
            background = GetComponent<Image>();
        if (cardButton == null)
            cardButton = GetComponent<Button>();
        if (selectedHighlight == null)
            selectedHighlight = transform.Find("SelectedHighlight")?.gameObject;
        if (rarityBorder == null)
            rarityBorder = transform.Find("Border")?.GetComponent<Image>();
        if (rewardIcon == null)
            rewardIcon = transform.Find("Icon")?.GetComponent<Image>();
        if (rewardTypeText == null)
            rewardTypeText = transform.Find("RewardType")?.GetComponent<TMP_Text>();
        if (rewardNameText == null)
            rewardNameText = transform.Find("RewardName")?.GetComponent<TMP_Text>();
        if (rarityText == null)
            rarityText = transform.Find("RarityText")?.GetComponent<TMP_Text>();
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    private static Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => new Color32(128, 128, 128, 255),
            ItemRarity.Uncommon => new Color32(63, 115, 199, 255),
            ItemRarity.Rare => new Color32(125, 69, 184, 255),
            ItemRarity.Legendary => new Color32(214, 169, 40, 255),
            _ => Color.white
        };
    }
}
