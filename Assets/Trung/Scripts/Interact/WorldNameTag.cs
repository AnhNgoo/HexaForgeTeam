using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldNameTag : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private string displayName;

    [Header("Quest Status Icon UI")]
    [SerializeField] private GameObject questIconRoot;
    [SerializeField] private Image questStatusImage;
    [SerializeField] private Sprite newQuestIcon;      // Icon dấu !
    [SerializeField] private Sprite claimQuestIcon;    // Icon dấu ?

    private Camera targetCamera;
    private Transform camTransform;

    private void Awake()
    {
        FindActiveCamera();

        if (nameText != null)
        {
            nameText.SetTextSafe(displayName);
        }
    }

    private void OnEnable()
    {
        FindActiveCamera();
    }

    private void FindActiveCamera()
    {
        if (targetCamera == null || !targetCamera.gameObject.activeInHierarchy)
        {
            targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (targetCamera != null)
            {
                camTransform = targetCamera.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (camTransform == null)
        {
            FindActiveCamera();
            if (camTransform == null) return;
        }

        // Ép mặt trước của Canvas luôn hướng thẳng vuông góc về phía Camera (chống Backface Culling)
        transform.rotation = Quaternion.LookRotation(transform.position - camTransform.position);
    }

    public void SetDisplayName(string newName)
    {
        displayName = newName;
        if (nameText != null)
        {
            nameText.SetTextSafe(displayName);
        }
    }

    public void UpdateQuestIcon(QuestState state)
    {
        if (questIconRoot == null || questStatusImage == null) return;

        if (state == QuestState.NotStarted && newQuestIcon != null)
        {
            questStatusImage.sprite = newQuestIcon;
            questStatusImage.enabled = true;
            questIconRoot.SetActive(true);

            // Cưỡng chế hiển thị
            EnsureVisibleHierarchy(questIconRoot);
        }
        else if (state == QuestState.CanClaim && claimQuestIcon != null)
        {
            questStatusImage.sprite = claimQuestIcon;
            questStatusImage.enabled = true;
            questIconRoot.SetActive(true);

            // Cưỡng chế hiển thị
            EnsureVisibleHierarchy(questIconRoot);
        }
        else
        {
            HideQuestIcon();
        }
    }

    private void EnsureVisibleHierarchy(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.enabled = true;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 50; // Đẩy layer lên cao nhất để không bị model/tường đè
        }
    }

    public void HideQuestIcon()
    {
        if (questIconRoot != null)
        {
            questIconRoot.SetActive(false);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (nameText != null)
        {
            nameText.SetTextSafe(displayName);
        }
    }
#endif
}