using UnityEngine;
using UnityEngine.UI;

public class LobbyMenuInteractionZone : MonoBehaviour
{
    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private Button interactionButton;
    [SerializeField] private bool useDistanceCheck = true;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Primary Interaction")]
    [SerializeField] private KeyCode primaryKey = KeyCode.F;
    [SerializeField] private MenuType primaryMenu = MenuType.StoreMenu;
    [SerializeField] private bool useKeyboardShortcut;

    [Header("Secondary Interaction")]
    [SerializeField] private bool useSecondaryInteraction;
    [SerializeField] private KeyCode secondaryKey = KeyCode.G;
    [SerializeField] private MenuType secondaryMenu =
        MenuType.InventoryRuneMenu;

    private bool playerInside;
    private void Start()
    {
        CacheInteractionButton();
        BindInteractionButton();
        CachePlayer();
        SetInteractionUI(false);
    }

    private void OnDestroy()
    {
        UnbindInteractionButton();
    }

    private void Update()
    {
        LobbyUIOverlayManager manager =
            LobbyUIOverlayManager.Instance;

        if (useDistanceCheck)
        {
            UpdatePlayerInsideByDistance();
        }

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
            if (useKeyboardShortcut &&
                (Input.GetKeyDown(primaryKey) ||
                 (useSecondaryInteraction &&
                  Input.GetKeyDown(secondaryKey))))
            {
                Debug.LogError(
                    "Không tìm thấy LobbyUIOverlayManager. " +
                    "Hãy tạo GameObject LobbyUIOverlayManager " +
                    "trong scene Demo Lobby và gắn script."
                );
            }

            return;
        }

        if (useKeyboardShortcut &&
            Input.GetKeyDown(primaryKey))
        {
            OpenPrimaryMenu();
            return;
        }

        if (useKeyboardShortcut &&
            useSecondaryInteraction &&
            Input.GetKeyDown(secondaryKey))
        {
            manager.OpenMenu(secondaryMenu);
        }
    }

    private void CacheInteractionButton()
    {
        if (interactionButton != null)
            return;

        if (interactionUI == null)
            return;

        interactionButton =
            interactionUI.GetComponent<Button>();

        if (interactionButton == null)
        {
            interactionButton =
                interactionUI.GetComponentInChildren<Button>(true);
        }
    }

    private void BindInteractionButton()
    {
        if (interactionButton == null)
            return;

        interactionButton.onClick.RemoveListener(OpenPrimaryMenu);
        interactionButton.onClick.AddListener(OpenPrimaryMenu);
    }

    private void UnbindInteractionButton()
    {
        if (interactionButton == null)
            return;

        interactionButton.onClick.RemoveListener(OpenPrimaryMenu);
    }

    private void OpenPrimaryMenu()
    {
        LobbyUIOverlayManager manager =
            LobbyUIOverlayManager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                "Không tìm thấy LobbyUIOverlayManager. " +
                "Hãy tạo GameObject LobbyUIOverlayManager trong scene Lobby."
            );

            return;
        }

        SetInteractionUI(false);
        manager.OpenMenu(primaryMenu);
    }

    private void CachePlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void UpdatePlayerInsideByDistance()
    {
        if (player == null)
        {
            CachePlayer();
        }

        if (player == null)
        {
            playerInside = false;
            return;
        }

        float sqrDistance =
            (player.position - transform.position).sqrMagnitude;

        float sqrInteractionDistance =
            interactionDistance * interactionDistance;

        playerInside = sqrDistance <= sqrInteractionDistance;
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
