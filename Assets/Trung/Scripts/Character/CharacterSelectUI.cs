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

    [Header("Coming Soon Badges (Kéo thả Badge/Text của từng nút vào đây)")]
    [SerializeField] private GameObject kaelComingSoonBadge;
    [SerializeField] private GameObject lyraComingSoonBadge;
    [SerializeField] private GameObject aresComingSoonBadge;
    [SerializeField] private GameObject elaraComingSoonBadge;

    [Header("Character Info")]
    [SerializeField] private TMP_Text CharacterNameText;
    [SerializeField] private TMP_Text RoleText;
    [SerializeField] private TMP_Text StatText;
    [SerializeField] private TMP_Text DescriptionText;

    [SerializeField] private Image RuneIcon1;
    [SerializeField] private Image RuneIcon2;
    [SerializeField] private Image RuneIcon3;

    [Header("Action Buttons")]
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button BuildRuneButton;

    [Header("Rune Icons")]
    [SerializeField] private Sprite RedRuneSprite;
    [SerializeField] private Sprite GreenRuneSprite;
    [SerializeField] private Sprite BlueRuneSprite;

    [Header("Coming Soon Config")]
    [Tooltip("Thêm các nhân vật chưa ra mắt vào đây (Mặc định: Ares, Elara)")]
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
        // 1. Cập nhật Bật/Tắt nhãn Coming Soon cho từng nút chọn
        bool isKaelCS = comingSoonCharacters.Contains(CharacterType.Kael);
        bool isLyraCS = comingSoonCharacters.Contains(CharacterType.Lyra);
        bool isAresCS = comingSoonCharacters.Contains(CharacterType.Ares);
        bool isElaraCS = comingSoonCharacters.Contains(CharacterType.Elara);

        if (kaelComingSoonBadge != null) kaelComingSoonBadge.SetActive(isKaelCS);
        if (lyraComingSoonBadge != null) lyraComingSoonBadge.SetActive(isLyraCS);
        if (aresComingSoonBadge != null) aresComingSoonBadge.SetActive(isAresCS);
        if (elaraComingSoonBadge != null) elaraComingSoonBadge.SetActive(isElaraCS);

        // 2. Cho phép click các nút để xem thông tin/stat (hoặc khóa nút tùy chọn)
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

        // ĐỒNG BỘ MÔ HÌNH 3D
        CharacterPreviewManager preview = FindFirstObjectByType<CharacterPreviewManager>();
        if (preview != null)
        {
            preview.RefreshPreview(previewingCharacter);
        }

        bool isPreviewingCS = comingSoonCharacters.Contains(previewingCharacter);
        bool isCurrentDeployed = (previewingCharacter == deployedChar);

        // Nút Confirm chỉ hiện khi: Không phải nhân vật đang dùng VÀ Không phải con Coming Soon
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

            if (DescriptionText != null)
            {
                string skill1Name = realData.skill1Data != null ? realData.skill1Data.skillName : "Skill 1";
                string skill2Name = realData.skill2Data != null ? realData.skill2Data.skillName : "Skill 2";
                DescriptionText.SetTextSafe($"Skill 1: {skill1Name}\nSkill 2: {skill2Name}");
            }
        }
        else
        {
            if (CharacterNameText != null) CharacterNameText.SetTextSafe(type.ToString().ToUpper());
            if (RoleText != null) RoleText.SetTextSafe("Unknown");
            if (StatText != null) StatText.SetTextSafe("HP: ---\nATK: ---\nDEF: ---\nSPD: ---");
            if (DescriptionText != null) DescriptionText.SetTextSafe("No character data found.");
        }

        if (CharacterManager.Instance != null)
        {
            CharacterRuneEquip currentBuild = CharacterManager.Instance.GetCharacterRuneBuild(type);
            Image[] targetIcons = new Image[3] { RuneIcon1, RuneIcon2, RuneIcon3 };

            for (int i = 0; i < targetIcons.Length; i++)
            {
                if (targetIcons[i] == null) continue;

                if (currentBuild != null && !string.IsNullOrEmpty(currentBuild.equippedRuneIDs[i]))
                {
                    string runeID = currentBuild.equippedRuneIDs[i];
                    RuneData equippedRune = null;

                    if (RuneInventoryManager.Instance != null)
                    {
                        for (int k = 0; k < RuneInventoryManager.Instance.runes.Count; k++)
                        {
                            if (RuneInventoryManager.Instance.runes[k].runeID == runeID)
                            {
                                equippedRune = RuneInventoryManager.Instance.runes[k];
                                break;
                            }
                        }
                    }

                    if (equippedRune != null)
                    {
                        targetIcons[i].color = Color.white;
                        if (equippedRune.runeColor == RuneColor.Red) targetIcons[i].sprite = RedRuneSprite;
                        if (equippedRune.runeColor == RuneColor.Green) targetIcons[i].sprite = GreenRuneSprite;
                        if (equippedRune.runeColor == RuneColor.Blue) targetIcons[i].sprite = BlueRuneSprite;
                    }
                }
                else
                {
                    targetIcons[i].color = new Color(1f, 1f, 1f, 0.2f);
                    if (type == CharacterType.Kael) targetIcons[i].sprite = (i == 2) ? GreenRuneSprite : RedRuneSprite;
                    if (type == CharacterType.Lyra) targetIcons[i].sprite = (i == 0) ? RedRuneSprite : BlueRuneSprite;
                    if (type == CharacterType.Ares) targetIcons[i].sprite = (i == 2) ? BlueRuneSprite : GreenRuneSprite;
                    if (type == CharacterType.Elara) targetIcons[i].sprite = (i == 0) ? RedRuneSprite : (i == 1) ? GreenRuneSprite : BlueRuneSprite;
                }
            }
        }
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
}