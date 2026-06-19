using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterMenu : MenuBase
{
    public override MenuType menuType => MenuType.CharacterMenu;

    [Serializable]
    public class CharacterItem
    {
        [Header("Character")]
        public string characterId;
        public string characterName;

        [Header("UI")]
        public Button btn_Character;
        public Sprite fullBodySprite;

        [Tooltip("Khung hoặc dấu hiển thị avatar đang được chọn")]
        public GameObject selectedMark;

        [NonSerialized]
        public UnityAction buttonAction;
    }

    [Header("Characters")]
    [SerializeField]
    private List<CharacterItem> characters = new List<CharacterItem>();

    [Header("Character Display")]
    [SerializeField] private Image img_Character;
    [SerializeField] private TextMeshProUGUI txt_CharacterName;

    [Header("Buttons")]
    [SerializeField] private Button btn_Select;
    [SerializeField] private Button btn_Back;

    [Header("Navigation")]
    [SerializeField] private MenuType backMenu = MenuType.TitleMenu;

    private int selectedIndex = -1;

    private const string SelectedCharacterKey = "SelectedCharacterId";

    protected override void LoadComponent()
    {
        // Nên kéo thả trực tiếp trong Inspector.
    }

    protected override void LoadComponentRuntime()
    {
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        AddEvents();
        ShowSavedCharacter();
    }

    public override void Close()
    {
        RemoveEvents();

        base.Close();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackButtonClicked();
        }
    }

    private void AddEvents()
    {
        RemoveEvents();

        for (int i = 0; i < characters.Count; i++)
        {
            int index = i;

            CharacterItem character = characters[i];

            if (character == null || character.btn_Character == null)
                continue;

            character.buttonAction = () => ShowCharacter(index);

            character.btn_Character.onClick.AddListener(
                character.buttonAction
            );
        }

        if (btn_Select != null)
            btn_Select.onClick.AddListener(OnSelectButtonClicked);

        if (btn_Back != null)
            btn_Back.onClick.AddListener(OnBackButtonClicked);
    }

    private void RemoveEvents()
    {
        foreach (CharacterItem character in characters)
        {
            if (character == null ||
                character.btn_Character == null ||
                character.buttonAction == null)
            {
                continue;
            }

            character.btn_Character.onClick.RemoveListener(
                character.buttonAction
            );

            character.buttonAction = null;
        }

        if (btn_Select != null)
            btn_Select.onClick.RemoveListener(OnSelectButtonClicked);

        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);
    }

    private void ShowSavedCharacter()
    {
        if (characters.Count == 0)
        {
            ClearDisplay();
            return;
        }

        string savedId = PlayerPrefs.GetString(
            SelectedCharacterKey,
            ""
        );

        int savedIndex = characters.FindIndex(
            character =>
                character != null &&
                character.characterId == savedId
        );

        if (savedIndex >= 0)
        {
            ShowCharacter(savedIndex);
        }
        else
        {
            ShowCharacter(0);
        }
    }

    private void ShowCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
            return;

        CharacterItem character = characters[index];

        if (character == null)
            return;

        selectedIndex = index;

        if (img_Character != null)
        {
            img_Character.sprite = character.fullBodySprite;
            img_Character.enabled =
                character.fullBodySprite != null;

            img_Character.preserveAspect = true;
        }

        if (txt_CharacterName != null)
        {
            txt_CharacterName.text =
                character.characterName;
        }

        UpdateSelectedMark();

        Debug.Log(
            "Đang xem nhân vật: " +
            character.characterName
        );
    }

    private void UpdateSelectedMark()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterItem character = characters[i];

            if (character == null ||
                character.selectedMark == null)
            {
                continue;
            }

            character.selectedMark.SetActive(
                i == selectedIndex
            );
        }
    }

    private void OnSelectButtonClicked()
    {
        if (selectedIndex < 0 ||
            selectedIndex >= characters.Count)
        {
            Debug.LogWarning("Chưa chọn nhân vật.");
            return;
        }

        CharacterItem selectedCharacter =
            characters[selectedIndex];

        if (selectedCharacter == null)
            return;

        PlayerPrefs.SetString(
            SelectedCharacterKey,
            selectedCharacter.characterId
        );

        PlayerPrefs.Save();

        CharacterSelectionData.SelectedCharacterId =
            selectedCharacter.characterId;

        Debug.Log(
            "Đã chọn nhân vật: " +
            selectedCharacter.characterName
        );

        Time.timeScale = 1f;

        LoadingData.TargetMenu = MenuType.GameplayMenu;

        UIManager.Instance.ChangeMenu(
            MenuType.LoadingMenu
        );
    }

    private void OnBackButtonClicked()
    {
        if (LobbyUIOverlayManager.Instance != null)
        {
            LobbyUIOverlayManager.Instance.CloseMenu();
            return;
        }

        UIManager.Instance.ChangeMenu(
            MenuType.TitleMenu
        );
    }

    private void ClearDisplay()
    {
        selectedIndex = -1;

        if (img_Character != null)
        {
            img_Character.sprite = null;
            img_Character.enabled = false;
        }

        if (txt_CharacterName != null)
            txt_CharacterName.text = "";

        if (btn_Select != null)
            btn_Select.interactable = false;
    }
}

public static class CharacterSelectionData
{
    public static string SelectedCharacterId;
}