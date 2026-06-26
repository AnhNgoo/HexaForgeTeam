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
public enum RuneColor
{
    Red,
    Green,
    Blue
}
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
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void Start()
    {
        SetupButtons();

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
            if (kaelHighlight != null)
     RefreshCharacterInfo(selected);
{
    kaelHighlight.SetActive(
        selected == CharacterType.Kael);
}

if (lyraHighlight != null)
{
    lyraHighlight.SetActive(
        selected == CharacterType.Lyra);
}

if (aresHighlight != null)
{
    aresHighlight.SetActive(
        selected == CharacterType.Ares);
}

if (elaraHighlight != null)
{
    elaraHighlight.SetActive(
        selected == CharacterType.Elara);
}

        if (StatusText != null)
        {
            StatusText.text =
                $"Selected: {selected}";
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

    private void SelectCharacter(
        CharacterType type)
    {
        if (!CharacterManager.Instance
            .IsUnlocked(type))
        {
            if (StatusText != null)
            {
                StatusText.text =
                    "Character Locked";
            }

            return;
        }

        CharacterManager.Instance
            .SelectCharacter(type);
        CharacterPreviewManager preview =
    FindFirstObjectByType<CharacterPreviewManager>();

if (preview != null)
{
    preview.RefreshPreview();
}

        if (StatusText != null)
        {
            StatusText.text =
                $"Selected: {type}";
        }

        RefreshUI();
    }
    private void RefreshCharacterInfo(
    CharacterType type)
{
    switch (type)
    {
        case CharacterType.Kael:

            CharacterNameText.text = "KAEL";
            RoleText.text = "⚔ Chiến Binh";

            StatText.text =
                "HP: 1350\n" +
                "ATK: 400\n" +
                "DEF: 160\n" +
                "SPD: 430";

            DescriptionText.text =
                "Crit Rate cao, dựa vào Dodge";

            SetRuneIcons(
                RuneColor.Red,
                RuneColor.Red,
                RuneColor.Green);

            break;

        case CharacterType.Lyra:

            CharacterNameText.text = "LYRA";
            RoleText.text = "🔮 Pháp Sư";

            StatText.text =
                "HP: 1200\n" +
                "MATK: 480\n" +
                "DEF: 130\n" +
                "SPD: 290";

            DescriptionText.text =
                "Burst DMG lớn, cần giữ khoảng cách";

            SetRuneIcons(
                RuneColor.Red,
                RuneColor.Blue,
                RuneColor.Blue);

            break;

        case CharacterType.Ares:

            CharacterNameText.text = "ARES";
            RoleText.text = "🛡 Đỡ Đòn";

            StatText.text =
                "HP: 2800\n" +
                "ATK: 220\n" +
                "DEF: 550\n" +
                "SPD: 220";

            DescriptionText.text =
                "Tanky, Parry lấy năng lượng";

            SetRuneIcons(
                RuneColor.Green,
                RuneColor.Green,
                RuneColor.Blue);

            break;

        case CharacterType.Elara:

            CharacterNameText.text = "ELARA";
            RoleText.text = "⚖ Hybrid";

            StatText.text =
                "HP: 2000\n" +
                "MATK: 280\n" +
                "DEF: 250\n" +
                "SPD: 320";

            DescriptionText.text =
                "Tự hồi máu, buff hỗ trợ";

            SetRuneIcons(
                RuneColor.Red,
                RuneColor.Green,
                RuneColor.Blue);

            break;
    }
}
private void SetRuneIcons(
    RuneColor rune1,
    RuneColor rune2,
    RuneColor rune3)
{
    RuneIcon1.sprite =
        GetRuneSprite(rune1);

    RuneIcon2.sprite =
        GetRuneSprite(rune2);

    RuneIcon3.sprite =
        GetRuneSprite(rune3);
}
private Sprite GetRuneSprite(
    RuneColor color)
{
    switch (color)
    {
        case RuneColor.Red:
            return RedRuneSprite;

        case RuneColor.Green:
            return GreenRuneSprite;

        case RuneColor.Blue:
            return BlueRuneSprite;
    }

    return null;
}
}