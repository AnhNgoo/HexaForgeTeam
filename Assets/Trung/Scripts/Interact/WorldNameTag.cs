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
    [SerializeField] private Sprite newQuestIcon;
    [SerializeField] private Sprite followQuestIcon;
    [SerializeField] private Sprite claimQuestIcon;
    [SerializeField] private Sprite gambleQuestIcon;

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

    public void UpdateQuestIcon(QuestState state, bool isGuiding = false, bool isGamble = false)
    {
        if (questIconRoot == null || questStatusImage == null) return;

        if (isGamble && state == QuestState.InProgress && gambleQuestIcon != null)
        {
            questStatusImage.sprite = gambleQuestIcon;
            questStatusImage.enabled = true;
            questIconRoot.SetActive(true);
            EnsureVisibleHierarchy(questIconRoot);
        }
        else if (state == QuestState.NotStarted && newQuestIcon != null)
        {
            questStatusImage.sprite = newQuestIcon;
            questStatusImage.enabled = true;
            questIconRoot.SetActive(true);
            EnsureVisibleHierarchy(questIconRoot);
        }
        else if (state == QuestState.InProgress && isGuiding && followQuestIcon != null)
        {
            questStatusImage.sprite = followQuestIcon;
            questStatusImage.enabled = true;
            questIconRoot.SetActive(true);
            EnsureVisibleHierarchy(questIconRoot);
        }
        else if (state == QuestState.CanClaim && claimQuestIcon != null)
        {
            questStatusImage.sprite = claimQuestIcon;
            questStatusImage.enabled = true;
            questIconRoot.SetActive(true);
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
            canvas.sortingOrder = 50;
        }
    }

    public void HideQuestIcon()
    {
        if (questIconRoot != null)
        {
            questIconRoot.SetActive(false);
        }
    }
}