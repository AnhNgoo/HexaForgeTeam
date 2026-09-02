using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuneDetailInfoPanel : MonoBehaviour
{
    public static RuneDetailInfoPanel Instance;

    [Header("Panel Root")]
    [SerializeField] private GameObject panelRoot; 

    [Header("Text Fields")]
    [SerializeField] private TMP_Text runeNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text colorText;
    [SerializeField] private TMP_Text affixListText; 
    [SerializeField] private TMP_Text loreText;

    [Header("Buttons")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button rerollPanelButton;

    [Header("Rune Visual Target (1 Image Duy Nhất)")]
    [SerializeField] private Image targetRuneImage;

    [Header("Rune Sprites (12 Sprites Ngọc Thường)")]
    [Tooltip("Thứ tự sắp xếp: Common(Red, Green, Blue) -> Rare(Red, Green, Blue) -> Epic(Red, Green, Blue) -> Legendary(Red, Green, Blue)")]
    [SerializeField] private List<Sprite> runeSprites = new List<Sprite>();
    
    [Header("Special Sprite (Ngọc Cổ Tự Ultimate)")]
    [SerializeField] private Sprite ultimateRuneSprite;

    private RuneData currentData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonClick);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
        }

        if (rerollPanelButton != null)
        {
            rerollPanelButton.onClick.RemoveAllListeners();
            rerollPanelButton.onClick.AddListener(() =>
            {
                if (currentData != null && RuneRerollUI.Instance != null)
                {
                    RuneRerollUI.Instance.OpenPanel(currentData);
                }
            });
        }
    }

    private void Update()
    {
        if (!IsPanelActive()) return;

        if (Input.GetMouseButtonDown(0))
        {
            GameObject targetObj = panelRoot != null ? panelRoot : gameObject;
            RectTransform rectTransform = targetObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                bool clickInsidePanel = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null);
                if (!clickInsidePanel)
                {
                    ClosePanel();
                }
            }
        }
    }

    public bool IsPanelActive()
    {
        if (panelRoot != null) return panelRoot.activeInHierarchy;
        return gameObject.activeInHierarchy;
    }

    public void DisplayRuneInfo(RuneData runeData)
    {
        if (runeData == null)
        {
            ClosePanel();
            return;
        }

        currentData = runeData;
        if (panelRoot != null) panelRoot.SetActive(true);
        else gameObject.SetActive(true);

        Color rarityColor = GetRarityColor(runeData.runeRarity);
        
        if (runeNameText != null)
        {
            runeNameText.text = runeData.runeName.ToUpper();
            runeNameText.color = rarityColor;
        }

        if (rarityText != null)
        {
            rarityText.text = $"RARITY: {runeData.runeRarity.ToString().ToUpper()}";
            rarityText.color = rarityColor;
        }

        if (colorText != null)
        {
            if (IsUltimateRune(runeData))
            {
                colorText.text = "ELEMENT: ORIGIN POWER";
                colorText.color = new Color(1f, 0.84f, 0f);
            }
            else
            {
                colorText.text = $"ELEMENT: {runeData.runeColor.ToString().ToUpper()}";
                colorText.color = (runeData.runeColor == RuneColor.Red) ? Color.red : (runeData.runeColor == RuneColor.Green) ? Color.green : Color.cyan;
            }
        }

        if (affixListText != null)
        {
            affixListText.text = "";
            for (int i = 0; i < runeData.affixes.Count; i++)
            {
                RuneAffixData affix = runeData.affixes[i];
                bool isPercent = IsPercentStat(affix.statType);
                string valueSign = affix.value >= 0 ? "+" : "";
                string valueFormat = isPercent ? $"{affix.value:F1}%" : $"{affix.value:F0}";
                affixListText.text += $"- {GetFullStatName(affix.statType)} : <color=#00FFCC>{valueSign}{valueFormat}</color>\n\n";
            }
        }

        if (loreText != null)
        {
            loreText.text = string.IsNullOrEmpty(runeData.runeLore) ? "" : $"<i>\"{runeData.runeLore}\"</i>";
        }

        UpdateActionText();
        UpdateRuneImageVisual(runeData);
    }

    public void ClosePanel()
    {
        currentData = null;
        if (panelRoot != null) panelRoot.SetActive(false);
        else gameObject.SetActive(false);

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.DeselectLockedRune();
        }
    }

    private void UpdateActionText()
    {
        if (actionButtonText == null || currentData == null) return;

        bool isEquippedByCurrentChar = false;
        bool isEquippedByOtherChar = false;
        string ownerCharName = "";

        CharacterType currentType = (RuneEquipUI.Instance != null) ? RuneEquipUI.Instance.GetViewingCharacter() : CharacterManager.Instance.GetSelectedCharacter();

        if (CharacterManager.Instance != null)
        {
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                if (build != null && build.equippedRuneIDs != null)
                {
                    for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                    {
                        if (build.equippedRuneIDs[i] == currentData.runeID)
                        {
                            if (charType == currentType)
                            {
                                isEquippedByCurrentChar = true;
                            }
                            else
                            {
                                isEquippedByOtherChar = true;
                                ownerCharName = charType.ToString().ToUpper();
                            }
                            break;
                        }
                    }
                }
            }
        }

        if (isEquippedByCurrentChar)
        {
            actionButtonText.text = "UNEQUIP";
            if (actionButton != null) actionButton.interactable = true;
        }
        else if (isEquippedByOtherChar)
        {
            actionButtonText.text = $"{ownerCharName}";
            if (actionButton != null) actionButton.interactable = false;
        }
        else
        {
            actionButtonText.text = "USE";
            if (actionButton != null) actionButton.interactable = true;
        }
    }

    private void OnActionButtonClick()
    {
        if (currentData == null || RuneInventoryManager.Instance == null) return;

        bool isEquippedByCurrentChar = false;
        bool isEquippedByOtherChar = false;
        string ownerCharName = "";

        CharacterType currentType = (RuneEquipUI.Instance != null) ? RuneEquipUI.Instance.GetViewingCharacter() : CharacterManager.Instance.GetSelectedCharacter();
        
        if (CharacterManager.Instance != null)
        {
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                var b = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                if (b != null && b.equippedRuneIDs != null)
                {
                    for (int i = 0; i < b.equippedRuneIDs.Length; i++)
                    {
                        if (b.equippedRuneIDs[i] == currentData.runeID)
                        {
                            if (charType == currentType)
                            {
                                isEquippedByCurrentChar = true;
                            }
                            else
                            {
                                isEquippedByOtherChar = true;
                                ownerCharName = charType.ToString().ToUpper();
                            }
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
                LobbyNotifyManager.Instance.ShowNotify($"Rune is currently equipped by {ownerCharName}!", Color.red);
            }
            return;
        }

        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (!isEquippedByCurrentChar)
        {
            int targetSlotIndex = -1;
            if (build != null && RuneEquipUI.Instance != null)
            {
                if (IsUltimateRune(currentData))
                {
                    for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(build.equippedRuneIDs[i])) { targetSlotIndex = i; break; }
                    }
                }
                else
                {
                    for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(build.equippedRuneIDs[i]))
                        {
                            RuneColor requiredColor = RuneEquipUI.Instance.GetSlotRequiredColor(currentType, i);
                            if (currentData.runeColor == requiredColor) 
                            { 
                                targetSlotIndex = i; 
                                break; 
                            }
                        }
                    }
                }
            }

            if (targetSlotIndex == -1)
            {
                if (LobbyNotifyManager.Instance != null) 
                    LobbyNotifyManager.Instance.ShowNotify("Rune element mismatch or no vacant slots left!", Color.yellow);
                return;
            }

            RuneInventoryManager.Instance.EquipRune(currentData, currentType);
        }
        else
        {
            RuneInventoryManager.Instance.UnequipRune(currentData, currentType);
        }

        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
        UpdateActionText();
    }

    private void OnDeleteButtonClick()
    {
        if (currentData == null) return;

        int refundGem = 30;
        int shardReward = 50;

        switch (currentData.runeRarity)
        {
            case RuneRarity.Rare: refundGem = 80; shardReward = 150; break;
            case RuneRarity.Epic: refundGem = 200; shardReward = 400; break;
            case RuneRarity.Legendary: refundGem = 600; shardReward = 1000; break;
        }

        CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
        foreach (CharacterType charType in allChars)
        {
            var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
            if (build != null)
            {
                for (int slot = 0; slot < build.equippedRuneIDs.Length; slot++)
                {
                    if (build.equippedRuneIDs[slot] == currentData.runeID)
                    {
                        build.equippedRuneIDs[slot] = "";
                    }
                }
            }
        }

        if (GemManager.Instance != null && refundGem > 0)
        {
            GemManager.Instance.AddGem(refundGem);
        }

        if (RuneShardManager.Instance != null && shardReward > 0)
        {
            RuneShardManager.Instance.AddShards(shardReward);
        }

        if (RuneInventoryManager.Instance != null)
        {
            RuneInventoryManager.Instance.RemoveRune(currentData.runeID);
        }

        ClosePanel();

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.RefreshInventory();
        }

        if (RuneEquipUI.Instance != null)
        {
            RuneEquipUI.Instance.RefreshEquipUI();
        }

        if (LobbyStatManager.Instance != null)
        {
            LobbyStatManager.Instance.RecalculateStats();
        }

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify($"Dismantled Rune! Recovered {refundGem} Gems & {shardReward} Shards.", Color.green);
        }
    }

    private void UpdateRuneImageVisual(RuneData runeData)
    {
        if (targetRuneImage == null) return;

        if (IsUltimateRune(runeData))
        {
            if (ultimateRuneSprite != null)
            {
                targetRuneImage.gameObject.SetActive(true);
                targetRuneImage.sprite = ultimateRuneSprite;
            }
            return;
        }

        int targetIndex = ((int)runeData.runeRarity * 3) + (int)runeData.runeColor;
        if (targetIndex >= 0 && targetIndex < runeSprites.Count && runeSprites[targetIndex] != null)
        {
            targetRuneImage.gameObject.SetActive(true);
            targetRuneImage.sprite = runeSprites[targetIndex];
        }
        else
        {
            targetRuneImage.gameObject.SetActive(false);
        }
    }

    #region Helpers
    private Color GetRarityColor(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return Color.white;
            case RuneRarity.Rare: return new Color(0.2f, 0.6f, 1f);
            case RuneRarity.Epic: return new Color(0.7f, 0.2f, 1f);
            case RuneRarity.Legendary: return new Color(1f, 0.5f, 0f);
        }
        return Color.white;
    }

    private bool IsPercentStat(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HPPercent:
            case RuneStatType.MPPercent:
            case RuneStatType.StaminaPercent:
            case RuneStatType.ATKPercent:
            case RuneStatType.DEFPercent:
                return true;
        }
        return false;
    }

    private string GetFullStatName(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP: return "Max HP";
            case RuneStatType.HPPercent: return "HP Modifier";
            case RuneStatType.MP: return "Max MP";
            case RuneStatType.MPPercent: return "MP Modifier";
            case RuneStatType.MPRegen: return "MP Regeneration";
            case RuneStatType.Stamina: return "Stamina Cap";
            case RuneStatType.StaminaPercent: return "Stamina Modifier";
            case RuneStatType.StaminaRegen: return "Stamina Regeneration";
            case RuneStatType.ATK: return "Attack Power";
            case RuneStatType.ATKPercent: return "Attack Modifier";
            case RuneStatType.DEF: return "Defense Rating";
            case RuneStatType.DEFPercent: return "Defense Modifier";
            case RuneStatType.Speed: return "Movement Speed";
            case RuneStatType.PoisonDamage: return "Poison Damage";
            case RuneStatType.AllStats: return "All Attributes";
        }
        return "Unknown Stat";
    }

    private bool IsUltimateRune(RuneData rune)
    {
        if (rune == null) return false;
        for (int i = 0; i < rune.affixes.Count; i++)
        {
            if (rune.affixes[i].statType == RuneStatType.AllStats) return true;
        }
        return false;
    }
    #endregion

    public RuneData GetRuneDataHelper() => currentData;
}