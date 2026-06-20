using UnityEngine;
using UnityEngine.UI;

public class UIPanelCloseButton : LoadComponents
{
    [SerializeField]
    private Button button;

    protected override void LoadComponent()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }
    }

    protected override void LoadComponentRuntime()
    {
    }

    private void Start()
    {
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            ClosePanel);
    }

    private void ClosePanel()
    {
        if (LobbyPanelManager.Instance == null)
        {
            return;
        }

        LobbyPanelManager.Instance
            .CloseCurrentPanel();
    }
}