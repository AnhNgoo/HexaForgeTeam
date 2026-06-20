using UnityEngine;

public class LobbyMenuInteractionZone : MonoBehaviour
{
    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionUI;

    [Header("Primary Interaction")]
    [SerializeField] private KeyCode primaryKey = KeyCode.F;
    [SerializeField] private MenuType primaryMenu = MenuType.StoreMenu;

    [Header("Secondary Interaction")]
    [SerializeField] private bool useSecondaryInteraction;
    [SerializeField] private KeyCode secondaryKey = KeyCode.G;
    [SerializeField] private MenuType secondaryMenu =
        MenuType.InventoryGemMenu;

    private bool playerInside;

    private void Start()
    {
        SetInteractionUI(false);
    }

    private void Update()
    {
        LobbyUIOverlayManager manager =
            LobbyUIOverlayManager.Instance;

        // Đang đứng ngoài vùng thì luôn tắt UI.
        if (!playerInside)
        {
            SetInteractionUI(false);
            return;
        }

        /*
         * Khi Player đang trong vùng:
         * - Nếu chưa có manager, vẫn hiện UI để kiểm tra trigger.
         * - Nếu UI scene đang mở/load thì ẩn lời nhắc.
         */
        bool canShowInteraction =
            manager == null ||
            (!manager.IsUIOpen && !manager.IsBusy);

        SetInteractionUI(canShowInteraction);

        if (!canShowInteraction)
            return;

        if (manager == null)
        {
            if (Input.GetKeyDown(primaryKey) ||
                (useSecondaryInteraction &&
                 Input.GetKeyDown(secondaryKey)))
            {
                Debug.LogError(
                    "Không tìm thấy LobbyUIOverlayManager. " +
                    "Hãy tạo GameObject LobbyUIOverlayManager " +
                    "trong scene Demo Lobby và gắn script."
                );
            }

            return;
        }

        if (Input.GetKeyDown(primaryKey))
        {
            manager.OpenMenu(primaryMenu);
            return;
        }

        if (useSecondaryInteraction &&
            Input.GetKeyDown(secondaryKey))
        {
            manager.OpenMenu(secondaryMenu);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = true;
        SetInteractionUI(true);

        Debug.Log(
            "Player entered interaction zone: " +
            gameObject.name
        );
    }

    private void OnTriggerStay(Collider other)
    {
        // Giúp khôi phục trạng thái nếu script được bật khi Player
        // đã đứng sẵn bên trong trigger.
        if (!IsPlayer(other))
            return;

        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = false;
        SetInteractionUI(false);

        Debug.Log(
            "Player exited interaction zone: " +
            gameObject.name
        );
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        // Kiểm tra collider hiện tại và toàn bộ object cha.
        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void SetInteractionUI(bool state)
    {
        if (interactionUI == null)
            return;

        if (interactionUI.activeSelf != state)
            interactionUI.SetActive(state);
    }

    private void OnDisable()
    {
        SetInteractionUI(false);
    }
}