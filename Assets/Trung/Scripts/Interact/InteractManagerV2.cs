using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractManagerV2 : MonoBehaviour
{
    public static InteractManagerV2 Instance;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private bool enableMouseWheel = true;
    [SerializeField] private float scrollCooldown = 0.12f;

    [Header("Distance Limit")]
    [SerializeField] private float maxInteractDistance = 3.2f;

    private float nextScrollTime;
    private float cooldownUntilTime = 0f;
    private bool consumedInputThisFrame;

    private readonly List<InteractV2> interactObjects = new List<InteractV2>();
    private int currentIndex = 0;
    private Transform playerTransform;

    public bool IsBusy { get; set; }
    public IReadOnlyList<InteractV2> InteractObjects => interactObjects;

    public InteractV2 CurrentInteract
    {
        get
        {
            CleanupInvalidInteracts();
            if (interactObjects.Count == 0) return null;
            currentIndex = Mathf.Clamp(currentIndex, 0, interactObjects.Count - 1);
            return interactObjects[currentIndex];
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += ForceRefresh;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= ForceRefresh;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ForceUnlockState();
    }

    public void ForceUnlockState()
    {
        IsBusy = false;
        cooldownUntilTime = 0f;
        currentIndex = 0;
        interactObjects.Clear();
        playerTransform = null;

        if (InteractUIV2.Instance != null)
        {
            InteractUIV2.Instance.Hide();
        }

        CancelInvoke(nameof(RescanNearbyInteracts));
        Invoke(nameof(RescanNearbyInteracts), 0.2f);
    }

    public void SetCooldown(float duration)
    {
        cooldownUntilTime = Time.unscaledTime + duration;
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    private void CleanupInvalidInteracts()
    {
        FindPlayer();

        for (int i = interactObjects.Count - 1; i >= 0; i--)
        {
            var item = interactObjects[i];
            if (item == null || 
                item.gameObject == null || 
                !item.enabled || 
                !item.gameObject.activeInHierarchy || 
                !item.IsFeatureUnlocked())
            {
                interactObjects.RemoveAt(i);
                continue;
            }

            if (playerTransform != null)
            {
                Collider col = item.GetComponent<Collider>();
                Vector3 closestPoint = col != null ? col.ClosestPoint(playerTransform.position) : item.transform.position;
                float dist = Vector3.Distance(closestPoint, playerTransform.position);

                if (dist > maxInteractDistance)
                {
                    interactObjects.RemoveAt(i);
                }
            }
        }

        if (currentIndex >= interactObjects.Count)
        {
            currentIndex = Mathf.Max(0, interactObjects.Count - 1);
        }
    }

    private void Update()
    {
        CleanupInvalidInteracts();

        bool isDialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen();
        bool isMenuOpen = UIManager.Instance != null && 
                          UIManager.Instance.CurrentMenuType != MenuType.DefaultLobbyInputMenu && 
                          UIManager.Instance.CurrentMenuType != MenuType.GameplayMenu && 
                          UIManager.Instance.CurrentMenuType != MenuType.None;

        if (isDialogueActive || isMenuOpen)
        {
            IsBusy = true;
        }
        else if (IsBusy && !isDialogueActive && !isMenuOpen)
        {
            IsBusy = false;
        }

        if (interactObjects.Count == 0 || IsBusy || Time.unscaledTime < cooldownUntilTime)
        {
            if (InteractUIV2.Instance != null && InteractUIV2.Instance.gameObject.activeSelf)
            {
                InteractUIV2.Instance.Hide();
            }
            return;
        }

        // Xử lý cuộn chuột khi có từ 2 đối tượng trở lên
        if (enableMouseWheel && interactObjects.Count > 1)
        {
            HandleMouseWheel();
        }

        if (Input.GetKeyDown(interactKey))
        {
            ExecuteCurrent();
        }

        RefreshUI();
    }

    public void RescanNearbyInteracts()
    {
        FindPlayer();
        if (playerTransform == null) return;

        Collider[] hits = Physics.OverlapSphere(playerTransform.position, maxInteractDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            InteractV2 interact = hits[i].GetComponent<InteractV2>();
            if (interact != null && interact.enabled && interact.gameObject.activeInHierarchy && interact.IsFeatureUnlocked())
            {
                Register(interact);
            }
        }
        ForceRefresh();
    }

    public void Register(InteractV2 interact)
    {
        if (interact == null || !interact.IsFeatureUnlocked() || interactObjects.Contains(interact)) return;

        interactObjects.Add(interact);
        SortInteracts();
        RefreshUI();
    }

    public void Unregister(InteractV2 interact)
    {
        if (interact == null) return;

        interactObjects.Remove(interact);
        if (currentIndex >= interactObjects.Count)
        {
            currentIndex = Mathf.Max(0, interactObjects.Count - 1);
        }

        if (interactObjects.Count == 0)
        {
            if (InteractUIV2.Instance != null) InteractUIV2.Instance.Hide();
        }
        else
        {
            RefreshUI();
        }
    }

    private void SortInteracts()
    {
        FindPlayer();
        if (playerTransform == null) return;

        interactObjects.Sort((a, b) =>
        {
            if (a.Priority != b.Priority) return b.Priority.CompareTo(a.Priority);

            Collider colA = a.GetComponent<Collider>();
            Collider colB = b.GetComponent<Collider>();

            Vector3 ptA = colA != null ? colA.ClosestPoint(playerTransform.position) : a.transform.position;
            Vector3 ptB = colB != null ? colB.ClosestPoint(playerTransform.position) : b.transform.position;

            return Vector3.Distance(ptA, playerTransform.position).CompareTo(Vector3.Distance(ptB, playerTransform.position));
        });
    }

    private void HandleMouseWheel()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.05f && Time.unscaledTime >= nextScrollTime)
        {
            if (scroll < 0f)
            {
                Next();
            }
            else
            {
                Previous();
            }
            nextScrollTime = Time.unscaledTime + scrollCooldown;
        }
    }

    public void Next()
    {
        if (interactObjects.Count <= 1) return;
        currentIndex = (currentIndex + 1) % interactObjects.Count;
        RefreshUI();
    }

    public void Previous()
    {
        if (interactObjects.Count <= 1) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = interactObjects.Count - 1;
        RefreshUI();
    }

    public void ForceRefresh()
    {
        CleanupInvalidInteracts();
        if (interactObjects.Count == 0)
        {
            if (InteractUIV2.Instance != null) InteractUIV2.Instance.Hide();
            return;
        }
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (IsBusy || interactObjects.Count == 0)
        {
            if (InteractUIV2.Instance != null) InteractUIV2.Instance.Hide();
            return;
        }

        for (int i = 0; i < interactObjects.Count; i++)
        {
            if (interactObjects[i] != null)
            {
                interactObjects[i].SetSelected(i == currentIndex);
            }
        }

        if (InteractUIV2.Instance != null)
        {
            InteractUIV2.Instance.Refresh(interactObjects, currentIndex);
        }
    }

    private void LateUpdate()
    {
        consumedInputThisFrame = false;
    }

    public void ExecuteCurrent()
    {
        if (IsBusy || Time.unscaledTime < cooldownUntilTime) return;

        InteractV2 target = CurrentInteract;
        if (target != null)
        {
            consumedInputThisFrame = true;
            target.Execute();
        }
    }

    public bool WasInputConsumedThisFrame() => consumedInputThisFrame;
}