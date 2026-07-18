using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : LoadComponents
{
    [SerializeField]
    private TMP_Text StatusText;

    [Header("Character Buttons")]
    [SerializeField]
    private Button KaelButton;

    [SerializeField]
    private Button LyraButton;

    [SerializeField]
    private Button AresButton;

    [SerializeField]
    private Button ElaraButton;
    [SerializeField]
private GameObject kaelHighlight;

[SerializeField]
private GameObject lyraHighlight;

[SerializeField]
private GameObject aresHighlight;

[SerializeField]
private GameObject elaraHighlight;
[Header("Character Info")]
[SerializeField]
private TMP_Text CharacterNameText;

[SerializeField]
private TMP_Text RoleText;

[SerializeField]
private TMP_Text StatText;

[SerializeField]
private TMP_Text DescriptionText;

[SerializeField]
private Image RuneIcon1;

[SerializeField]
private Image RuneIcon2;

[SerializeField]
private Image RuneIcon3;
[SerializeField]
    private Button BuildRuneButton;

[Header("Rune Icons")]
[SerializeField]
private Sprite RedRuneSprite;

[SerializeField]
private Sprite GreenRuneSprite;

[SerializeField]
private Sprite BlueRuneSprite;

    protected override void LoadComponent()
    {
        if (StatusText == null)
        {
            StatusText =
                transform.Find(nameof(StatusText))
                ?.GetComponent<TMP_Text>();
        }

        if (KaelButton == null)
        {
            KaelButton =
                transform.Find(nameof(KaelButton))
                ?.GetComponent<Button>();
        }

        if (LyraButton == null)
        {
            LyraButton =
                transform.Find(nameof(LyraButton))
                ?.GetComponent<Button>();
        }

        if (AresButton == null)
        {
            AresButton =
                transform.Find(nameof(AresButton))
                ?.GetComponent<Button>();
        }

        if (ElaraButton == null)
        {
            ElaraButton =
                transform.Find(nameof(ElaraButton))
                ?.GetComponent<Button>();
        }
        if (BuildRuneButton == null)
    {
        BuildRuneButton = transform.Find(nameof(BuildRuneButton))?.GetComponent<Button>();
    }
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void Start()
    {
        SetupButtons();

        RefreshUI();
    }
    private void OnEnable()
    {
        RefreshUI();
    }

    private void SetupButtons()
    {
        if (KaelButton != null)
        {
            KaelButton.onClick.RemoveAllListeners();
            KaelButton.onClick.AddListener(
                SelectKael);
        }

        if (LyraButton != null)
        {
            LyraButton.onClick.RemoveAllListeners();
            LyraButton.onClick.AddListener(
                SelectLyra);
        }

        if (AresButton != null)
        {
            AresButton.onClick.RemoveAllListeners();
            AresButton.onClick.AddListener(
                SelectAres);
        }

        if (ElaraButton != null)
        {
            ElaraButton.onClick.RemoveAllListeners();
            ElaraButton.onClick.AddListener(
                SelectElara);
        }
        if (BuildRuneButton != null)
    {
        BuildRuneButton.onClick.RemoveAllListeners();
        BuildRuneButton.onClick.AddListener(OnBuildRuneClicked);
    }
    }

    public void RefreshUI()
    {
        if (KaelButton != null)
        {
            KaelButton.interactable = true;
        }

        if (LyraButton != null)
        {
            LyraButton.interactable =
                CharacterManager.Instance
                .IsUnlocked(
                    CharacterType.Lyra);
        }

        if (AresButton != null)
        {
            AresButton.interactable =
                CharacterManager.Instance
                .IsUnlocked(
                    CharacterType.Ares);
        }

        if (ElaraButton != null)
        {
            ElaraButton.interactable =
                CharacterManager.Instance
                .IsUnlocked(
                    CharacterType.Elara);
        }

        CharacterType selected =
            CharacterManager.Instance
            .GetSelectedCharacter();
            RefreshCharacterInfo(selected);

    if (kaelHighlight != null)
    {
        kaelHighlight.SetActive(selected == CharacterType.Kael);
    }

    if (lyraHighlight != null)
    {
        lyraHighlight.SetActive(selected == CharacterType.Lyra);
    }

    if (aresHighlight != null)
    {
        aresHighlight.SetActive(selected == CharacterType.Ares);
    }

    if (elaraHighlight != null)
    {
        elaraHighlight.SetActive(selected == CharacterType.Elara);
    }

        if (StatusText != null)
        {
            StatusText.SetTextSafe(
                $"Selected: {selected}");
        }
    }

    public void SelectKael()
    {
        SelectCharacter(
            CharacterType.Kael);
    }

    public void SelectLyra()
    {
        SelectCharacter(
            CharacterType.Lyra);
    }

    public void SelectAres()
    {
        SelectCharacter(
            CharacterType.Ares);
    }

    public void SelectElara()
    {
        SelectCharacter(
            CharacterType.Elara);
    }

    private void SelectCharacter(CharacterType type)
{
    if (!CharacterManager.Instance.IsUnlocked(type))
    {
        if (StatusText != null)
        {
            StatusText.SetTextSafe("<color=#FF4C4C>CHARACTER LOCKED</color>");
        }
        return;
    }

    CharacterManager.Instance.SelectCharacter(type);
    CharacterPreviewManager preview = FindFirstObjectByType<CharacterPreviewManager>();

    if (preview != null)
    {
        preview.RefreshPreview();
    }

    if (StatusText != null)
    {
        StatusText.SetTextSafe($"<color=#00FFCC>DEPLOYED: {type.ToString().ToUpper()}</color>");
    }

    RefreshUI();
}
    private void RefreshCharacterInfo(CharacterType type)
{
    switch (type)
    {
        case CharacterType.Kael:

            CharacterNameText.SetTextSafe("KAEL");
            RoleText.SetTextSafe("Chiến Binh");

            StatText.SetTextSafe(
                "HP: 1350\n" +
                "ATK: 400\n" +
                "DEF: 160\n" +
                "SPD: 430");

            DescriptionText.SetTextSafe(
                "Crit Rate cao, dựa vào Dodge");

            break;

        case CharacterType.Lyra:

            CharacterNameText.SetTextSafe("LYRA");
            RoleText.SetTextSafe("Pháp Sư");

            StatText.SetTextSafe(
                "HP: 1200\n" +
                "MATK: 480\n" +
                "DEF: 130\n" +
                "SPD: 290");

            DescriptionText.SetTextSafe(
                "Burst DMG lớn, cần giữ khoảng cách");

            break;

        case CharacterType.Ares:

            CharacterNameText.SetTextSafe("ARES");
            RoleText.SetTextSafe("Đỡ Đòn");

            StatText.SetTextSafe(
                "HP: 2800\n" +
                "ATK: 220\n" +
                "DEF: 550\n" +
                "SPD: 220");

            DescriptionText.SetTextSafe(
                "Tanky, Parry lấy năng lượng");

            break;

        case CharacterType.Elara:

            CharacterNameText.SetTextSafe("ELARA");
            RoleText.SetTextSafe("Hybrid");

            StatText.SetTextSafe(
                "HP: 2000\n" +
                "MATK: 280\n" +
                "DEF: 250\n" +
                "SPD: 320");

            DescriptionText.SetTextSafe(
                "Tự hồi máu, buff hỗ trợ");

            break;
    }

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