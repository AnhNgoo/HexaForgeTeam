using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Character Info")]
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
    [Tooltip("Thứ tự Index chuẩn:\n0: Common Red, 1: Common Green, 2: Common Blue\n3: Rare Red, 4: Rare Green, 5: Rare Blue\n6: Epic Red, 7: Epic Green, 8: Epic Blue\n9: Legendary Red, 10: Legendary Green, 11: Legendary Blue")]
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
        bool isKaelCS = comingSoonCharacters.Contains(CharacterType.Kael);
        bool isLyraCS = comingSoonCharacters.Contains(CharacterType.Lyra);
        bool isAresCS = comingSoonCharacters.Contains(CharacterType.Ares);
        bool isElaraCS = comingSoonCharacters.Contains(CharacterType.Elara);

        if (kaelComingSoonBadge != null) kaelComingSoonBadge.SetActive(isKaelCS);
        if (lyraComingSoonBadge != null) lyraComingSoonBadge.SetActive(isLyraCS);
        if (aresComingSoonBadge != null) aresComingSoonBadge.SetActive(isAresCS);
        if (elaraComingSoonBadge != null) elaraComingSoonBadge.SetActive(isElaraCS);

        if (KaelButton != null) KaelButton.interactable = !isKaelCS;
        if (LyraButton != null) LyraButton.interactable = !isLyraCS;
        if (AresButton != null) AresButton.interactable = !isAresCS;
        if (ElaraButton != null) ElaraButton.interactable = !isElaraCS;

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

        bool isPreviewingCS = comingSoonCharacters.Contains(previewingCharacter);
        bool isCurrentDeployed = (previewingCharacter == deployedChar);

        if (ConfirmButton != null)
        {
            ConfirmButton.gameObject.SetActive(!isCurrentDeployed && !isPreviewingCS);
        }

        if (StatusText != null)
        {
            if (isPreviewingCS)
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
        Character enumChar = (type == CharacterType.Kael) ? Character.Kael : Character.Lyra;
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
                StatText.SetTextSafe(
                    $"HP: {realData.stats.maxHealth}\n" +
                    $"{atkLabel}: {realData.stats.damage}\n" +
                    $"DEF: {realData.stats.defense}\n" +
                    $"SPD: {realData.stats.speed}"
                );
            }

            // Gán Icon Skill 1 + Tự bổ sung Tooltip Trigger
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

            // Gán Icon Skill 2 + Tự bổ sung Tooltip Trigger
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
            if (StatText != null) StatText.SetTextSafe("HP: ---\nATK: ---\nDEF: ---\nSPD: ---");

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

                // Ô ĐÃ ĐEO NGỌC -> Hiện ảnh thật + Tooltip thông tin ngọc chi tiết
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
                        details += $"✦ {affix.statType}: <color=#00FFCC>{sign}{affix.value:F1}</color>\n";
                    }
                    if (!string.IsNullOrEmpty(equippedRune.runeLore)) details += $"\n<i>\"{equippedRune.runeLore}\"</i>";

                    trigger.SetData(title, details, targetIcons[i].sprite);
                }
                // Ô TRỐNG -> Hiện icon mờ + Tooltip hướng dẫn loại ngọc tương ứng
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