using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterSelectUI : LoadComponents
{
    [SerializeField] private TMP_Text StatusText;

    [Header("Character Buttons")]
    [SerializeField] private Button KaelButton;
    [SerializeField] private Button LyraButton;
    [SerializeField] private Button AresButton;
    [SerializeField] private Button ElaraButton;

    [Header("Highlights")]
    [SerializeField] private GameObject kaelHighlight;
    [SerializeField] private GameObject lyraHighlight;
    [SerializeField] private GameObject aresHighlight;
    [SerializeField] private GameObject elaraHighlight;

    [Header("Coming Soon Badges")]
    [SerializeField] private GameObject kaelComingSoonBadge;
    [SerializeField] private GameObject lyraComingSoonBadge;
    [SerializeField] private GameObject aresComingSoonBadge;
    [SerializeField] private GameObject elaraComingSoonBadge;

    [Header("Character Info Texts")]
    [SerializeField] private TMP_Text CharacterNameText;
    [SerializeField] private TMP_Text RoleText;
    [SerializeField] private TMP_Text StatText;

    [Header("Skill Icons")]
    [SerializeField] private Image Skill1IconImage;
    [SerializeField] private Image Skill2IconImage;

    [Header("Rune Slots Visual")]
    [SerializeField] private Image RuneIcon1;
    [SerializeField] private Image RuneIcon2;
    [SerializeField] private Image RuneIcon3;

    [Header("Action Buttons")]
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button BuildRuneButton;

    [Header("Rune Visuals List (12 Sprites Ngọc)")]
    [SerializeField] private List<Sprite> runeSprites = new List<Sprite>();

    [Header("Special Origin Rune Sprite")]
    [SerializeField] private Sprite originRuneSprite;

    [Header("Coming Soon Config")]
    [SerializeField] private List<CharacterType> comingSoonCharacters = new List<CharacterType>()
    {
        CharacterType.Ares,
        CharacterType.Elara
    };

    private CharacterType previewingCharacter;

    protected override void LoadComponent()
    {
        if (StatusText == null) StatusText = transform.Find(nameof(StatusText))?.GetComponent<TMP_Text>();
        if (KaelButton == null) KaelButton = transform.Find(nameof(KaelButton))?.GetComponent<Button>();
        if (LyraButton == null) LyraButton = transform.Find(nameof(LyraButton))?.GetComponent<Button>();
        if (AresButton == null) AresButton = transform.Find(nameof(AresButton))?.GetComponent<Button>();
        if (ElaraButton == null) ElaraButton = transform.Find(nameof(ElaraButton))?.GetComponent<Button>();

        if (ConfirmButton == null) ConfirmButton = transform.Find("ConfirmButton")?.GetComponent<Button>();
        if (BuildRuneButton == null) BuildRuneButton = transform.Find(nameof(BuildRuneButton))?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime() { }

    private void Start()
    {
        SetupButtons();
        ResetToCurrentDeployed();
    }

    private void OnEnable()
    {
        ResetToCurrentDeployed();
    }

    private void SetupButtons()
    {
        if (KaelButton != null)
        {
            KaelButton.onClick.RemoveAllListeners();
            KaelButton.onClick.AddListener(() => OnSelectCharacterClicked(CharacterType.Kael));
        }

        if (LyraButton != null)
        {
            LyraButton.onClick.RemoveAllListeners();
            LyraButton.onClick.AddListener(() => OnSelectCharacterClicked(CharacterType.Lyra));
        }

        if (AresButton != null)
        {
            AresButton.onClick.RemoveAllListeners();
            AresButton.onClick.AddListener(() => OnSelectCharacterClicked(CharacterType.Ares));
        }

        if (ElaraButton != null)
        {
            ElaraButton.onClick.RemoveAllListeners();
            ElaraButton.onClick.AddListener(() => OnSelectCharacterClicked(CharacterType.Elara));
        }

        if (ConfirmButton != null)
        {
            ConfirmButton.onClick.RemoveAllListeners();
            ConfirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (BuildRuneButton != null)
        {
            BuildRuneButton.onClick.RemoveAllListeners();
            BuildRuneButton.onClick.AddListener(OnBuildRuneClicked);
        }
    }

    public void ResetToCurrentDeployed()
    {
        if (CharacterManager.Instance != null)
        {
            previewingCharacter = CharacterManager.Instance.GetSelectedCharacter();
        }
        else
        {
            previewingCharacter = CharacterType.Kael;
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        bool isKaelUnlocked = CharacterManager.Instance == null || CharacterManager.Instance.IsUnlocked(CharacterType.Kael);
        bool isLyraUnlocked = CharacterManager.Instance != null && CharacterManager.Instance.IsUnlocked(CharacterType.Lyra);
        bool isAresUnlocked = CharacterManager.Instance != null && CharacterManager.Instance.IsUnlocked(CharacterType.Ares);
        bool isElaraUnlocked = CharacterManager.Instance != null && CharacterManager.Instance.IsUnlocked(CharacterType.Elara);

        bool isKaelCS = comingSoonCharacters.Contains(CharacterType.Kael) || !isKaelUnlocked;
        bool isLyraCS = comingSoonCharacters.Contains(CharacterType.Lyra) || !isLyraUnlocked;
        bool isAresCS = comingSoonCharacters.Contains(CharacterType.Ares) || !isAresUnlocked;
        bool isElaraCS = comingSoonCharacters.Contains(CharacterType.Elara) || !isElaraUnlocked;

        if (kaelComingSoonBadge != null) kaelComingSoonBadge.SetActive(isKaelCS);
        if (lyraComingSoonBadge != null) lyraComingSoonBadge.SetActive(isLyraCS);
        if (aresComingSoonBadge != null) aresComingSoonBadge.SetActive(isAresCS);
        if (elaraComingSoonBadge != null) elaraComingSoonBadge.SetActive(isElaraCS);

        if (KaelButton != null) KaelButton.interactable = isKaelUnlocked && !comingSoonCharacters.Contains(CharacterType.Kael);
        if (LyraButton != null) LyraButton.interactable = isLyraUnlocked && !comingSoonCharacters.Contains(CharacterType.Lyra);
        if (AresButton != null) AresButton.interactable = isAresUnlocked && !comingSoonCharacters.Contains(CharacterType.Ares);
        if (ElaraButton != null) ElaraButton.interactable = isElaraUnlocked && !comingSoonCharacters.Contains(CharacterType.Elara);

        CharacterType deployedChar = CharacterManager.Instance != null ? CharacterManager.Instance.GetSelectedCharacter() : CharacterType.Kael;

        RefreshCharacterInfo(previewingCharacter);

        if (kaelHighlight != null) kaelHighlight.SetActive(previewingCharacter == CharacterType.Kael);
        if (lyraHighlight != null) lyraHighlight.SetActive(previewingCharacter == CharacterType.Lyra);
        if (aresHighlight != null) aresHighlight.SetActive(previewingCharacter == CharacterType.Ares);
        if (elaraHighlight != null) elaraHighlight.SetActive(previewingCharacter == CharacterType.Elara);

        CharacterPreviewManager preview = FindFirstObjectByType<CharacterPreviewManager>();
        if (preview != null)
        {
            preview.RefreshPreview(previewingCharacter);
        }

        bool isPreviewUnlocked = CharacterManager.Instance == null || CharacterManager.Instance.IsUnlocked(previewingCharacter);
        bool isPreviewingCS = comingSoonCharacters.Contains(previewingCharacter) || !isPreviewUnlocked;
        bool isCurrentDeployed = (previewingCharacter == deployedChar);

        if (ConfirmButton != null)
        {
            ConfirmButton.gameObject.SetActive(true);
            ConfirmButton.transform.DOKill(true);
            ConfirmButton.transform.localScale = Vector3.one;

            TMP_Text btnText = ConfirmButton.GetComponentInChildren<TMP_Text>();

            if (isPreviewingCS)
            {
                ConfirmButton.interactable = false;
                if (btnText != null) 
                {
                    if (!isPreviewUnlocked) btnText.SetTextSafe("LOCKED (LV.5)");
                    else btnText.SetTextSafe("LOCKED");
                }
            }
            else if (isCurrentDeployed)
            {
                ConfirmButton.interactable = false;
                if (btnText != null) btnText.SetTextSafe("DEPLOYED");
            }
            else
            {
                ConfirmButton.interactable = true;
                if (btnText != null) btnText.SetTextSafe("DEPLOY HERO");
            }
        }

        if (StatusText != null)
        {
            if (!isPreviewUnlocked)
            {
                StatusText.SetTextSafe($"<color=#FF3333>REACH ACCOUNT LEVEL 5 TO UNLOCK {previewingCharacter.ToString().ToUpper()}</color>");
            }
            else if (comingSoonCharacters.Contains(previewingCharacter))
            {
                StatusText.SetTextSafe($"<color=#FF9900>COMING SOON: {previewingCharacter.ToString().ToUpper()}</color>");
            }
            else if (isCurrentDeployed)
            {
                StatusText.SetTextSafe($"<color=#00FFCC>DEPLOYED: {previewingCharacter.ToString().ToUpper()}</color>");
            }
            else
            {
                StatusText.SetTextSafe($"<color=#FFFF66>PREVIEWING: {previewingCharacter.ToString().ToUpper()}</color>");
            }
        }
    }

    private void OnSelectCharacterClicked(CharacterType type)
    {
        if (CharacterManager.Instance != null && !CharacterManager.Instance.IsUnlocked(type))
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Reach Account Level 5 to unlock {type}!", Color.yellow);
            }
            previewingCharacter = type;
            RefreshUI();
            return;
        }

        if (comingSoonCharacters.Contains(type))
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"{type} is coming soon!", Color.yellow);
            }
            return;
        }

        previewingCharacter = type;
        RefreshUI();
    }

    private void OnConfirmClicked()
    {
        if (comingSoonCharacters.Contains(previewingCharacter)) return;
        if (CharacterManager.Instance != null && !CharacterManager.Instance.IsUnlocked(previewingCharacter)) return;

        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.SelectCharacter(previewingCharacter);
        }

        if (PlayerManager.Instance != null)
        {
            Character pChar = (previewingCharacter == CharacterType.Kael) ? Character.Kael : Character.Lyra;
            PlayerManager.Instance.SelectCharacter(pChar);
            PlayerManager.Instance.SpawnCharacterInCurrentPosition();
        }

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify($"Hero {previewingCharacter} Deployed Successfully!", Color.green);
        }

        RefreshUI();
    }

    private void RefreshCharacterInfo(CharacterType type)
    {
        Character enumChar = Character.Kael;
        if (type == CharacterType.Lyra) enumChar = Character.Lyra;

        CharacterData realData = null;

        if (PlayerManager.Instance != null)
        {
            realData = PlayerManager.Instance.SelectCharacter(enumChar);
        }

        if (realData != null)
        {
            if (CharacterNameText != null) 
                CharacterNameText.SetTextSafe(string.IsNullOrEmpty(realData.characterName) ? type.ToString().ToUpper() : realData.characterName.ToUpper());

            if (RoleText != null) 
                RoleText.SetTextSafe(realData.characterTypes.ToString());

            if (StatText != null && realData.stats != null)
            {
                string atkLabel = (realData.characterTypes == CharacterTypes.Magical) ? "MATK" : "ATK";

float bonusHP = 0, bonusATK = 0, bonusDEF = 0, bonusMP = 0, bonusStamina = 0, bonusCrit = 0;
                
                if (CharacterManager.Instance != null && RuneInventoryManager.Instance != null)
                {
                    CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(type);
                    if (build != null && build.equippedRuneIDs != null)
                    {
                        for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                        {
                            string runeId = build.equippedRuneIDs[i];
                            if (string.IsNullOrEmpty(runeId)) continue;

                            RuneData rData = RuneInventoryManager.Instance.runes.Find(r => r.runeID == runeId);
                            if (rData != null && rData.affixes != null)
                            {
                                for (int k = 0; k < rData.affixes.Count; k++)
                                {
                                    var aff = rData.affixes[k];
                                    switch (aff.statType)
                                    {
                                        case RuneStatType.HP: bonusHP += aff.value; break;
                                        case RuneStatType.HPPercent: bonusHP += realData.stats.maxHealth * (aff.value / 100f); break;
                                        case RuneStatType.ATK: bonusATK += aff.value; break;
                                        case RuneStatType.ATKPercent: bonusATK += realData.stats.damage * (aff.value / 100f); break;
                                        case RuneStatType.DEF: bonusDEF += aff.value; break;
                                        case RuneStatType.DEFPercent: bonusDEF += realData.stats.defense * (aff.value / 100f); break;
                                        case RuneStatType.MP: bonusMP += aff.value; break;
                                        case RuneStatType.MPPercent: bonusMP += realData.stats.mp * (aff.value / 100f); break;
                                        case RuneStatType.Stamina: bonusStamina += aff.value; break;
                                        case RuneStatType.StaminaPercent: bonusStamina += realData.stats.stamina * (aff.value / 100f); break;
                                        case RuneStatType.CritChance: bonusCrit += aff.value; break;
                                        case RuneStatType.AllStats:
                                            bonusHP += realData.stats.maxHealth * (aff.value / 100f);
                                            bonusATK += realData.stats.damage * (aff.value / 100f);
                                            bonusDEF += realData.stats.defense * (aff.value / 100f);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                // Helper định dạng màu mè đẹp mắt: Base + (+Bonus)
                string FormatStatLine(string labelColor, string label, float baseVal, float bonusVal)
                {
                    if (bonusVal > 0.1f)
                    {
                        float totalVal = baseVal + bonusVal;
                        return $"<color={labelColor}><b>{label}:</b></color> {baseVal:F0} <color=#00FFCC><b>(+{bonusVal:F0})</b></color> <color=#FFFFFF>→</color> <b><color=#FFD700>{totalVal:F0}</color></b>";
                    }
                    return $"<color={labelColor}><b>{label}:</b></color> <b>{baseVal:F0}</b>";
                }

                StatText.SetTextSafe(
                    $"{FormatStatLine("#FF5555", "HP", realData.stats.maxHealth, bonusHP)}\n" +
                    $"{FormatStatLine("#55AAFF", "MP", realData.stats.mp, bonusMP)}\n" +
                    $"{FormatStatLine("#FFAA33", atkLabel, realData.stats.damage, bonusATK)}\n" +
                    $"{FormatStatLine("#33FFBB", "DEF", realData.stats.defense, bonusDEF)}\n" +
                    $"<color=#55FF55><b>SPD:</b></color> <b>{realData.stats.speed:F1}</b>\n" +
                    $"{FormatStatLine("#FFFF66", "Stamina", realData.stats.stamina, bonusStamina)}" +
                    (bonusCrit > 0.1f ? $"\n<color=#CC66FF><b>Crit Chance:</b></color> <color=#00FFCC><b>+{bonusCrit:F1}%</b></color>" : "")
                );
            }

            if (Skill1IconImage != null)
            {
                if (realData.skill1Data != null && realData.skill1Data.skillIcon != null)
                {
                    Skill1IconImage.sprite = realData.skill1Data.skillIcon;
                    Skill1IconImage.gameObject.SetActive(true);

                    var trigger = Skill1IconImage.GetComponent<UITooltipAutoTrigger>() ?? Skill1IconImage.gameObject.AddComponent<UITooltipAutoTrigger>();
                    trigger.SetSkillData(realData.skill1Data);
                }
                else
                {
                    Skill1IconImage.gameObject.SetActive(false);
                }
            }

            if (Skill2IconImage != null)
            {
                if (realData.skill2Data != null && realData.skill2Data.skillIcon != null)
                {
                    Skill2IconImage.sprite = realData.skill2Data.skillIcon;
                    Skill2IconImage.gameObject.SetActive(true);

                    var trigger = Skill2IconImage.GetComponent<UITooltipAutoTrigger>() ?? Skill2IconImage.gameObject.AddComponent<UITooltipAutoTrigger>();
                    trigger.SetSkillData(realData.skill2Data);
                }
                else
                {
                    Skill2IconImage.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (CharacterNameText != null) CharacterNameText.SetTextSafe(type.ToString().ToUpper());
            if (RoleText != null) RoleText.SetTextSafe("Unknown");
            if (StatText != null) StatText.SetTextSafe("HP: ---\nMP: ---\nATK: ---\nDEF: ---\nSPD: ---");

            if (Skill1IconImage != null) Skill1IconImage.gameObject.SetActive(false);
            if (Skill2IconImage != null) Skill2IconImage.gameObject.SetActive(false);
        }

        if (CharacterManager.Instance != null)
        {
            CharacterRuneEquip currentBuild = CharacterManager.Instance.GetCharacterRuneBuild(type);
            Image[] targetIcons = new Image[3] { RuneIcon1, RuneIcon2, RuneIcon3 };

            for (int i = 0; i < targetIcons.Length; i++)
            {
                if (targetIcons[i] == null) continue;

                string equippedRuneID = (currentBuild != null && currentBuild.equippedRuneIDs != null && i < currentBuild.equippedRuneIDs.Length) 
                    ? currentBuild.equippedRuneIDs[i] 
                    : "";

                RuneData equippedRune = null;
                if (!string.IsNullOrEmpty(equippedRuneID) && RuneInventoryManager.Instance != null)
                {
                    equippedRune = RuneInventoryManager.Instance.runes.Find(r => r.runeID == equippedRuneID);
                }

                var trigger = targetIcons[i].GetComponent<UITooltipAutoTrigger>() ?? targetIcons[i].gameObject.AddComponent<UITooltipAutoTrigger>();

                if (equippedRune != null)
                {
                    targetIcons[i].sprite = GetRealRuneSprite(equippedRune);
                    targetIcons[i].color = Color.white;

                    string title = $"<color={GetRarityHexColor(equippedRune.runeRarity)}>{equippedRune.runeName.ToUpper()}</color>";
                    string details = $"<b>Rarity:</b> {equippedRune.runeRarity} | <b>Element:</b> {equippedRune.runeColor}\n\n";
                    for (int k = 0; k < equippedRune.affixes.Count; k++)
                    {
                        var affix = equippedRune.affixes[k];
                        string sign = affix.value >= 0 ? "+" : "";
                        details += $"- {affix.statType}: <color=#00FFCC>{sign}{affix.value:F1}</color>\n";
                    }
                    if (!string.IsNullOrEmpty(equippedRune.runeLore)) details += $"\n<i>\"{equippedRune.runeLore}\"</i>";

                    trigger.SetData(title, details, targetIcons[i].sprite);
                }
                else
                {
                    RuneColor reqColor = (RuneEquipUI.Instance != null) 
                        ? RuneEquipUI.Instance.GetSlotRequiredColor(type, i) 
                        : (i == 0 ? RuneColor.Red : i == 1 ? RuneColor.Green : RuneColor.Blue);

                    int colorIndex = (reqColor == RuneColor.Red) ? 0 : (reqColor == RuneColor.Green) ? 1 : 2;

                    if (runeSprites != null && colorIndex < runeSprites.Count)
                    {
                        targetIcons[i].sprite = runeSprites[colorIndex];
                    }

                    targetIcons[i].color = new Color(1f, 1f, 1f, 0.25f);
                    trigger.SetData($"Empty Slot {i + 1}", $"Requires a <color={(reqColor == RuneColor.Red ? "red" : reqColor == RuneColor.Green ? "green" : "cyan")}>{reqColor}</color> Rune element.");
                }
            }
        }
    }

    private Sprite GetRealRuneSprite(RuneData rune)
    {
        if (rune == null) return null;

        if (rune.affixes != null)
        {
            for (int i = 0; i < rune.affixes.Count; i++)
            {
                if (rune.affixes[i].statType == RuneStatType.AllStats)
                {
                    return originRuneSprite != null ? originRuneSprite : (runeSprites.Count > 0 ? runeSprites[runeSprites.Count - 1] : null);
                }
            }
        }

        int colorOffset = (rune.runeColor == RuneColor.Red) ? 0 : (rune.runeColor == RuneColor.Green) ? 1 : 2;
        int targetIndex = ((int)rune.runeRarity * 3) + colorOffset;

        if (runeSprites != null && targetIndex >= 0 && targetIndex < runeSprites.Count)
        {
            return runeSprites[targetIndex];
        }

        return null;
    }

    private void OnBuildRuneClicked()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.ChangeMenu(MenuType.LobbyRuneInventoryMenu);

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.RefreshInventory();
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