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

        if (StatusText != null)
        {
            StatusText.text =
                $"Selected: {type}";
        }

        RefreshUI();
    }
}