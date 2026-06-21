using UnityEngine;

public class LobbyPanelManager : MonoBehaviour
{
    public static LobbyPanelManager Instance;

    [Header("Panels")]
    [SerializeField]
    private GameObject characterPanel;

    [SerializeField]
    private GameObject inventoryPanel;

    [SerializeField]
    private GameObject achievementPanel;

    [SerializeField]
    private GameObject gachaPanel;

    [SerializeField]
    private GameObject accountLevelPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CloseAllPanels();
    }

    public void OpenPanel(
        LobbyPanelType panelType)
    {
        CloseAllPanels();

        switch (panelType)
        {
            case LobbyPanelType.Character:

                characterPanel.SetActive(true);
                break;

            case LobbyPanelType.Inventory:

                inventoryPanel.SetActive(true);
                break;

            case LobbyPanelType.Achievement:

                achievementPanel.SetActive(true);
                break;

            case LobbyPanelType.Gacha:

                gachaPanel.SetActive(true);
                break;

            case LobbyPanelType.AccountLevel:

                accountLevelPanel.SetActive(true);
                break;
        }
    }

    public void CloseAllPanels()
    {
        if (characterPanel != null)
        {
            characterPanel.SetActive(false);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }

        if (gachaPanel != null)
        {
            gachaPanel.SetActive(false);
        }

        if (accountLevelPanel != null)
        {
            accountLevelPanel.SetActive(false);
        }
    }
    public void CloseCurrentPanel()
{
    if (characterPanel != null &&
        characterPanel.activeSelf)
    {
        characterPanel.SetActive(false);
    }

    if (inventoryPanel != null &&
        inventoryPanel.activeSelf)
    {
        inventoryPanel.SetActive(false);
    }

    if (achievementPanel != null &&
        achievementPanel.activeSelf)
    {
        achievementPanel.SetActive(false);
    }

    if (gachaPanel != null &&
        gachaPanel.activeSelf)
    {
        gachaPanel.SetActive(false);
    }

    if (accountLevelPanel != null &&
        accountLevelPanel.activeSelf)
    {
        accountLevelPanel.SetActive(false);
    }
}
}