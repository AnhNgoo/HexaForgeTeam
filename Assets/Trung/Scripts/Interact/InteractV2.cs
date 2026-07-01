using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class InteractV2 : MonoBehaviour
{
    [Header("Interaction")]

    [SerializeField]
    private string interactText = "Interact";

    [SerializeField]
    private Sprite interactIcon;

    [SerializeField]
    private int priority = 0;

    [Header("Trigger")]

    [SerializeField]
    private string playerTag = "Player";

    private bool playerInside;

    private bool isSelected;
    [SerializeField]
private bool openPanel;

[SerializeField]
private LobbyPanelType panelType;
    

    #region Property

    public string InteractText
    {
        get
        {
            return interactText;
        }
    }

    public Sprite InteractIcon
    {
        get
        {
            return interactIcon;
        }
    }

    public int Priority
    {
        get
        {
            return priority;
        }
    }

    public bool PlayerInside
    {
        get
        {
            return playerInside;
        }
    }

    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
    }

    #endregion

    #region Trigger

    private void Reset()
    {
        Collider col =
            GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(
        Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInside = true;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance
                .Register(this);
        }
    }

    private void OnTriggerExit(
        Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInside = false;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance
                .Unregister(this);
        }
    }

    #endregion

    #region Execute

    public virtual void Execute()
    {
        if (!playerInside)
        {
            return;
        }
        if (openPanel)
{
    LobbyPanelManager.Instance.OpenPanel(panelType);
    return;
}

        SendMessage(
            "OnInteract",
            SendMessageOptions.DontRequireReceiver);
    }

    #endregion

    #region Selected

    public void SetSelected(
        bool value)
    {
        if (isSelected == value)
        {
            return;
        }

        isSelected = value;

        if (value)
        {
            SendMessage(
                "OnSelected",
                SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            SendMessage(
                "OnDeselected",
                SendMessageOptions.DontRequireReceiver);
        }
    }

    #endregion

    #region Public

    public void SetInteractText(
        string value)
    {
        interactText = value;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance
                .ForceRefresh();
        }
    }

    #endregion

    private void OnDisable()
    {
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance
                .Unregister(this);
        }
    }

    private void OnDestroy()
    {
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance
                .Unregister(this);
        }
    }
}