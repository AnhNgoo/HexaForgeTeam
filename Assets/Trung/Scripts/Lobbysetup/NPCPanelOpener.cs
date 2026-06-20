using UnityEngine;

public class NPCPanelOpener : MonoBehaviour
{
    [SerializeField]
    private LobbyPanelType panelType;

    public void OnInteract()
    {
        if (LobbyPanelManager.Instance == null)
        {
            return;
        }

        LobbyPanelManager.Instance
            .OpenPanel(panelType);
    }
}