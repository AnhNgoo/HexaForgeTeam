using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitMenu : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    [Header("Confirmation")]
    [SerializeField] private GameObject confirmationRoot;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button btnQuit;
    [SerializeField] private Button btnCancel;

    [Header("Content")]
    [SerializeField, TextArea]
    private string confirmationMessage =
        "Are you sure you want to quit the game?";

    [SerializeField]
    private SystemSettingPage cancelReturnPage = SystemSettingPage.Audio;

    private bool eventsAdded;

    private void OnEnable()
    {
        AddEvents();
        ShowConfirmation();
    }

    private void OnDisable()
    {
        RemoveEvents();
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        if (btnQuit != null)
            btnQuit.onClick.AddListener(QuitGame);

        if (btnCancel != null)
            btnCancel.onClick.AddListener(Cancel);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        if (btnQuit != null)
            btnQuit.onClick.RemoveListener(QuitGame);

        if (btnCancel != null)
            btnCancel.onClick.RemoveListener(Cancel);

        eventsAdded = false;
    }

    public void ShowConfirmation()
    {
        if (descriptionText != null)
            descriptionText.text = confirmationMessage;

        if (confirmationRoot != null)
            confirmationRoot.SetActive(true);
    }

    public void Cancel()
    {
        if (confirmationRoot != null)
            confirmationRoot.SetActive(false);

        if (systemSettingsPanel != null)
            systemSettingsPanel.ShowPage(cancelReturnPage);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}