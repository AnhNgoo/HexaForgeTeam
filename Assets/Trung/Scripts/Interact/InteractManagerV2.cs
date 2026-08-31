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
    [SerializeField] private float maxInteractDistance = 3.5f;

    private float nextScrollTime;
    private float cooldownUntilTime = 0f;
    private float rescanTimer = 0f;
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

    public Transform GetPlayerTransform()
    {
        // Tự động tìm lại nhân vật mới ngay khi nhân vật cũ bị hủy (khi đổi tướng)
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
        return playerTransform;
    }

    private void CleanupInvalidInteracts()
    {
        Transform pTransform = GetPlayerTransform();

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

            if (pTransform != null)
            {
                Collider col = item.GetComponent<Collider>();
                Vector3 closestPoint = col != null ? col.ClosestPoint(pTransform.position) : item.transform.position;
                float dist = Vector3.Distance(closestPoint, pTransform.position);

                if (dist > maxInteractDistance + 0.5f)
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
        // Quét định kỳ mỗi 0.2s để đảm bảo không bao giờ bị mất tương tác khi đứng gần
        rescanTimer += Time.deltaTime;
        if (rescanTimer >= 0.2f)
        {
            rescanTimer = 0f;
            RescanNearbyInteracts();
        }

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
        Transform pTransform = GetPlayerTransform();
        if (pTransform == null) return;

        // Bật QueryTriggerInteraction.Collide để quét được cả Trigger Collider
        Collider[] hits = Physics.OverlapSphere(pTransform.position, maxInteractDistance, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            InteractV2 interact = hits[i].GetComponent<InteractV2>();
            if (interact != null && interact.enabled && interact.gameObject.activeInHierarchy && interact.IsFeatureUnlocked())
            {
                Register(interact);
            }
        }
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
        Transform pTransform = GetPlayerTransform();
        if (pTransform == null) return;

        interactObjects.Sort((a, b) =>
        {
            if (a.Priority != b.Priority) return b.Priority.CompareTo(a.Priority);

            Collider colA = a.GetComponent<Collider>();
            Collider colB = b.GetComponent<Collider>();

            Vector3 ptA = colA != null ? colA.ClosestPoint(pTransform.position) : a.transform.position;
            Vector3 ptB = colB != null ? colB.ClosestPoint(pTransform.position) : b.transform.position;

            return Vector3.Distance(ptA, pTransform.position).CompareTo(Vector3.Distance(ptB, pTransform.position));
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