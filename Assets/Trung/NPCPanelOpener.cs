using UnityEngine;

public class NPCPanelOpener : MonoBehaviour
{
    [SerializeField]
    private GameObject targetPanel;

    private void Start()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }

    public void OnInteract()
    {
        if (targetPanel == null)
        {
            return;
        }

        targetPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (targetPanel == null)
        {
            return;
        }

        targetPanel.SetActive(false);
    }
}