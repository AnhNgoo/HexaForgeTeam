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
    [SerializeField] private TMP_Text slot1ConditionText;
    [SerializeField] private TMP_Text slot2ConditionText;
    [SerializeField] private TMP_Text slot3ConditionText;

    [Header("Character Buttons")]
    [SerializeField] private GameObject kaelButtonObj;
    [SerializeField] private GameObject lyraButtonObj;
    [SerializeField] private GameObject aresButtonObj;
    [SerializeField] private GameObject elaraButtonObj;

    [Header("Coming Soon Config")]
    [SerializeField] private List<CharacterType> comingSoonCharacters = new List<CharacterType>()
    {
        CharacterType.Ares,
        CharacterType.Elara
    };

    private CharacterType viewingCharacter;

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

    [Header("Origin Rune")]
    [SerializeField] private Sprite originRuneSprite;

    public static RuneEquipUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (CharacterManager.Instance != null)
        {
            viewingCharacter = CharacterManager.Instance.GetSelectedCharacter();
        }

        if (comingSoonCharacters.Contains(viewingCharacter))
        {
            viewingCharacter = CharacterType.Kael;
        }

        RefreshEquipUI();
    }

    public void RefreshEquipUI()
    {
        ResetSlots();

        if (RuneInventoryManager.Instance == null)
        {
            return;
        }

        CharacterType currentType = viewingCharacter;
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);
        
        UpdateCharacterButtonsState();

        TMP_Text[] conditionTexts = new TMP_Text[3] { slot1ConditionText, slot2ConditionText, slot3ConditionText };
        for (int i = 0; i < conditionTexts.Length; i++)
        {
            if (conditionTexts[i] == null) continue;
            RuneColor requiredColor = GetSlotRequiredColor(currentType, i);
            conditionTexts[i].text = requiredColor.ToString();
            conditionTexts[i].color = (requiredColor == RuneColor.Red) ? Color.red : (requiredColor == RuneColor.Green) ? Color.green : Color.cyan;
        }

        if (build == null || build.equippedRuneIDs == null)
        {
            RefreshTotalStatText();
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            Image targetSlot = GetSlotImage(i);
            if (targetSlot == null) continue;

            var trigger = targetSlot.GetComponent<UITooltipAutoTrigger>() ?? targetSlot.gameObject.AddComponent<UITooltipAutoTrigger>();
            RuneColor requiredColor = GetSlotRequiredColor(currentType, i);

            string targetID = (i < build.equippedRuneIDs.Length) ? build.equippedRuneIDs[i] : "";

            if (string.IsNullOrEmpty(targetID))
            {
                targetSlot.sprite = emptySprite;
                targetSlot.color = new Color(1f, 1f, 1f, 0.3f);
                targetSlot.gameObject.SetActive(true);

                trigger.SetData($"Empty Slot {i + 1}", $"Requires a <color={(requiredColor == RuneColor.Red ? "red" : requiredColor == RuneColor.Green ? "green" : "cyan")}>{requiredColor}</color> Rune element.");
                continue;
            }

            RuneData rune = RuneInventoryManager.Instance.runes.Find(r => r.runeID == targetID);

            if (rune != null)
            {
                Sprite realRuneSprite = GetRuneSprite(rune);
                targetSlot.sprite = realRuneSprite;
                targetSlot.color = IsUltimateRune(rune) ? new Color(1f, 0.9f, 0.5f, 1f) : Color.white;
                targetSlot.gameObject.SetActive(true);

                string title = $"<color={GetRarityHexColor(rune.runeRarity)}>{rune.runeName.ToUpper()}</color>";
                string details = $"<b>Rarity:</b> {rune.runeRarity} | <b>Element:</b> {rune.runeColor}\n\n";

                if (rune.affixes != null)
                {
                    for (int a = 0; a < rune.affixes.Count; a++)
                    {
                        var affix = rune.affixes[a];
                        string sign = affix.value >= 0 ? "+" : "";
                        details += $"- {affix.statType}: <color=#00FFCC>{sign}{affix.value:F1}</color>\n";
                    }
                }

                if (!string.IsNullOrEmpty(rune.runeLore))
                {
                    details += $"\n<i>\"{rune.runeLore}\"</i>";
                }

                trigger.SetData(title, details, realRuneSprite);
            }
            else
            {
                targetSlot.sprite = emptySprite;
                targetSlot.color = new Color(1f, 1f, 1f, 0.3f);
                trigger.SetData($"Empty Slot {i + 1}", $"Requires a <color={(requiredColor == RuneColor.Red ? "red" : requiredColor == RuneColor.Green ? "green" : "cyan")}>{requiredColor}</color> Rune element.");
            }
        }

        RefreshTotalStatText();
    }

    #region Button Alpha, Lock & Coming Soon States

    private void UpdateCharacterButtonsState()
    {
        UpdateSingleButtonState(CharacterType.Kael, kaelButtonObj);
        UpdateSingleButtonState(CharacterType.Lyra, lyraButtonObj);
        UpdateSingleButtonState(CharacterType.Ares, aresButtonObj);
        UpdateSingleButtonState(CharacterType.Elara, elaraButtonObj);
    }

    private void UpdateSingleButtonState(CharacterType type, GameObject buttonObj)
    {
        if (buttonObj == null) return;

        buttonObj.SetActive(true);

        Button btn = buttonObj.GetComponent<Button>();
        Image img = buttonObj.GetComponent<Image>();

        bool isComingSoon = comingSoonCharacters.Contains(type);
        bool isUnlocked = (CharacterManager.Instance != null && CharacterManager.Instance.IsUnlocked(type)) && !isComingSoon;
        bool isSelected = (viewingCharacter == type);

        if (btn != null)
        {
            btn.interactable = !isComingSoon && isUnlocked;
        }

        var trigger = buttonObj.GetComponent<UITooltipAutoTrigger>() ?? buttonObj.gameObject.AddComponent<UITooltipAutoTrigger>();
        if (isComingSoon)
        {
            trigger.SetData($"{type} (Coming Soon)", "<color=#FF9900>Hero is under development and cannot equip runes yet!</color>");
        }

        if (img != null)
        {
            Color c = img.color;
            if (isComingSoon || !isUnlocked)
            {
                c.a = 0.25f;
            }
            else if (isSelected)
            {
                c.a = 0.6f;
            }
            else
            {
                c.a = 1.0f;
            }
            img.color = c;
        }
    }

    #endregion

    #region Switch Characters Action

    public void SwitchBuildToKael() => SwitchBuildToCharacter(CharacterType.Kael);
    public void SwitchBuildToLyra() => SwitchBuildToCharacter(CharacterType.Lyra);
    public void SwitchBuildToAres() => SwitchBuildToCharacter(CharacterType.Ares);
    public void SwitchBuildToElara() => SwitchBuildToCharacter(CharacterType.Elara);

    private void SwitchBuildToCharacter(CharacterType type)
    {
        if (comingSoonCharacters.Contains(type))
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"{type} is coming soon!", Color.yellow);
            }
            return;
        }

        viewingCharacter = type;
        RefreshEquipUI();

        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        if (RuneDetailInfoPanel.Instance != null && RuneDetailInfoPanel.Instance.gameObject.activeInHierarchy)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(RuneDetailInfoPanel.Instance.GetRuneDataHelper());
        }
    }

    #endregion

    #region Total Stats & Helpers

    private void RefreshTotalStatText()
    {
        if (totalStatText == null || RuneInventoryManager.Instance == null) return;

        Dictionary<RuneStatType, float> totalStats = RuneInventoryManager.Instance.GetStats(viewingCharacter);
        StringBuilder builder = new StringBuilder();

        foreach (var stat in totalStats)
        {
            if (stat.Key == RuneStatType.AllStats)
            {
                builder.AppendLine("<color=#FFD700>ORIGIN POWER\nAll Stats +" + $"{stat.Value:F0}</color>\n");
                continue;
            }
            
            bool isPercent = IsPercentStat(stat.Key);
            float rawValue = stat.Value;
            float cap = RuneInventoryManager.Instance.GetHardCap(stat.Key);
            bool reachedCap = rawValue >= cap;
            string color = reachedCap ? "#FF4C4C" : "#FFD966";
            float displayValue = Mathf.Min(rawValue, cap);

            if (isPercent)
            {
                builder.AppendLine($"<color={color}>{GetStatName(stat.Key)} +{displayValue:F1}% / {cap:F0}%</color>");
            }
            else
            {
                builder.AppendLine($"<color={color}>{GetStatName(stat.Key)} +{displayValue:F0} / {cap:F0}</color>");
            }
        }
        totalStatText.text = builder.ToString();
    }

    private void ResetSlots()
    {
        ResetSlot(slot1Image, 0);
        ResetSlot(slot2Image, 1);
        ResetSlot(slot3Image, 2);
    }

    private void ResetSlot(Image slotImage, int slotIndex)
    {
        if (slotImage == null) return;

        slotImage.sprite = emptySprite;
        Color color = slotImage.color;
        color.a = 0.3f;
        slotImage.color = color;
    }

    private Image GetSlotImage(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return slot1Image;
            case 1: return slot2Image;
            case 2: return slot3Image;
        }
        return null;
    }

    public void UnequipBySlot(int slotIndex)
    {
        if (RuneInventoryManager.Instance == null) return;

        CharacterType currentType = viewingCharacter; 
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (build == null || slotIndex < 0 || slotIndex >= build.equippedRuneIDs.Length) return;

        string targetID = build.equippedRuneIDs[slotIndex];
        if (string.IsNullOrEmpty(targetID)) return;

        RuneData rune = RuneInventoryManager.Instance.runes.Find(r => r.runeID == targetID);

        if (rune != null)
        {
            RuneInventoryManager.Instance.UnequipRune(rune, currentType);
            RefreshEquipUI();

            if (RuneInventoryUI.Instance != null)
            {
                RuneInventoryUI.Instance.RefreshInventory();
            }
        }
    }

    private Sprite GetRuneSprite(RuneData rune)
    {
        if (IsUltimateRune(rune)) return originRuneSprite;

        switch (rune.runeColor)
        {
            case RuneColor.Red: return GetRedSprite(rune.runeRarity);
            case RuneColor.Green: return GetGreenSprite(rune.runeRarity);
            case RuneColor.Blue: return GetBlueSprite(rune.runeRarity);
        }
        return null;
    }

    private Sprite GetRedSprite(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return redCommonSprite;
            case RuneRarity.Rare: return redRareSprite;
            case RuneRarity.Epic: return redEpicSprite;
            case RuneRarity.Legendary: return redLegendarySprite;
        }
        return null;
    }

    private Sprite GetGreenSprite(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return greenCommonSprite;
            case RuneRarity.Rare: return greenRareSprite;
            case RuneRarity.Epic: return greenEpicSprite;
            case RuneRarity.Legendary: return greenLegendarySprite;
        }
        return null;
    }

    private Sprite GetBlueSprite(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return blueCommonSprite;
            case RuneRarity.Rare: return blueRareSprite;
            case RuneRarity.Epic: return blueEpicSprite;
            case RuneRarity.Legendary: return blueLegendarySprite;
        }
        return null;
    }

    private bool IsPercentStat(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HPPercent: case RuneStatType.MPPercent: case RuneStatType.StaminaPercent:
            case RuneStatType.ATKPercent: case RuneStatType.DEFPercent: case RuneStatType.CritChance:
            case RuneStatType.CritDamage: case RuneStatType.ArmorPenetration: case RuneStatType.StaminaRegen:
                return true;
        }
        return false;
    }

    private string GetStatName(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP: return "HP";
            case RuneStatType.HPPercent: return "HP";
            case RuneStatType.MP: return "MP";
            case RuneStatType.MPPercent: return "MP";
            case RuneStatType.Stamina: return "Stamina";
            case RuneStatType.StaminaPercent: return "Stamina";
            case RuneStatType.ATK: return "ATK";
            case RuneStatType.ATKPercent: return "ATK";
            case RuneStatType.DEF: return "DEF";
            case RuneStatType.DEFPercent: return "DEF";
            case RuneStatType.CritChance: return "Crit Chance";
            case RuneStatType.CritDamage: return "Crit Damage";
            case RuneStatType.ArmorPenetration: return "Armor Penetration";
            case RuneStatType.StaminaRegen: return "Stamina Regen";
            case RuneStatType.AllStats: return "All Stats";
        }
        return "Unknown";
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

    private bool IsUltimateRune(RuneData rune)
    {
        if (rune == null) return false;
        for (int i = 0; i < rune.affixes.Count; i++)
        {
            if (rune.affixes[i].statType == RuneStatType.AllStats) return true;
        }
        return false;
    }

    public RuneColor GetSlotRequiredColor(CharacterType charType, int slotIndex)
    {
        if (charType == CharacterType.Kael) return (slotIndex == 2) ? RuneColor.Green : RuneColor.Red;
        if (charType == CharacterType.Lyra) return (slotIndex == 0) ? RuneColor.Red : RuneColor.Blue;
        if (charType == CharacterType.Ares) return (slotIndex == 2) ? RuneColor.Blue : RuneColor.Green;
        return (slotIndex == 0) ? RuneColor.Red : (slotIndex == 1) ? RuneColor.Green : RuneColor.Blue;
    }

    public CharacterType GetViewingCharacter() => viewingCharacter;

    #endregion

    public void AutoEquipBestRunes()
    {
        if (RuneInventoryManager.Instance == null || CharacterManager.Instance == null) return;

        CharacterType currentType = viewingCharacter;
        List<RuneData> availableRunes = new List<RuneData>(RuneInventoryManager.Instance.runes);

        CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
        foreach (CharacterType charType in allChars)
        {
            if (charType == currentType) continue;
            var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
            if (build != null && build.equippedRuneIDs != null)
            {
                foreach (string id in build.equippedRuneIDs)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        availableRunes.RemoveAll(r => r.runeID == id);
                    }
                }
            }
        }

        bool equippedAny = false;

        for (int slotIndex = 0; slotIndex < 3; slotIndex++)
        {
            RuneColor reqColor = GetSlotRequiredColor(currentType, slotIndex);

            RuneData bestRune = null;
            float maxScore = -1f;

            foreach (var rune in availableRunes)
            {
                bool isOrigin = IsUltimateRune(rune);
                if (!isOrigin && rune.runeColor != reqColor) continue;

                float score = ((int)rune.runeRarity * 100);
                if (rune.affixes != null)
                {
                    foreach (var affix in rune.affixes) score += affix.value;
                }

                if (score > maxScore)
                {
                    maxScore = score;
                    bestRune = rune;
                }
            }

            if (bestRune != null)
            {
                RuneInventoryManager.Instance.EquipRune(bestRune, currentType);
                availableRunes.Remove(bestRune);
                equippedAny = true;
            }
        }

        if (equippedAny)
        {
            RefreshEquipUI();
            if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
            if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Auto-equipped best runes build!", Color.green);
        }
        else
        {
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("No suitable runes found for auto-equip!", Color.yellow);
        }
    }
}