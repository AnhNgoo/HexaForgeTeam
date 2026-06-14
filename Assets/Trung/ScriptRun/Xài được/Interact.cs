using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{
    [Header("Interact")]
    [SerializeField] private string interactText = "Tương Tác";

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("UI")]
    [SerializeField] private GameObject interactButtonPrefab;

    [SerializeField] private Vector3 worldOffset =
        new Vector3(0f, 2f, 0f);

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange = false;

    private Camera mainCamera;

    private GameObject interactButtonObject;

    private Button interactButton;

    private TextMeshProUGUI txtInteract;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        UpdateButtonPosition();

        if (Input.GetKeyDown(interactKey))
        {
            InteractAction();
        }
    }

    private void LateUpdate()
    {
        if (!playerInRange)
            return;

        UpdateButtonPosition();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        ShowInteractButton();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;

        HideInteractButton();
    }

    private void ShowInteractButton()
    {
        if (interactButtonPrefab == null)
            return;

        if (interactButtonObject != null)
            return;

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas");
            return;
        }

        interactButtonObject = Instantiate(
            interactButtonPrefab,
            canvas.transform
        );

        interactButton =
            interactButtonObject.GetComponent<Button>();

        txtInteract =
            interactButtonObject.GetComponentInChildren<TextMeshProUGUI>();

        if (txtInteract != null)
        {
            txtInteract.text = interactText;
        }

        if (interactButton != null)
        {
            interactButton.onClick.RemoveAllListeners();

            interactButton.onClick.AddListener(InteractAction);
        }

        UpdateButtonPosition();
    }

    private void HideInteractButton()
    {
        if (interactButtonObject == null)
            return;

        Destroy(interactButtonObject);

        interactButtonObject = null;
    }

    private void UpdateButtonPosition()
    {
        if (interactButtonObject == null)
            return;

        if (mainCamera == null)
            return;

        Vector3 worldPosition =
            transform.position + worldOffset;

        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(worldPosition);

        interactButtonObject.transform.position = screenPosition;
    }

    public void InteractAction()
    {
        HideInteractButton();

        SendMessage(
            "OnInteract",
            SendMessageOptions.DontRequireReceiver
        );
    }

    private void OnDisable()
    {
        HideInteractButton();
    }

    private void OnDestroy()
    {
        HideInteractButton();
    }
}