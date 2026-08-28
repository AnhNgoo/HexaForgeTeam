using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractManagerV2 : MonoBehaviour
{
    public static InteractManagerV2 Instance;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private bool enableMouseWheel = true;

    [Header("Debug")]
    [SerializeField] private bool debugMode;
    [SerializeField] private float scrollCooldown = 0.15f;
    private bool consumedInputThisFrame;

    private float nextScrollTime;
    private float cooldownUntilTime = 0f;

    private readonly List<InteractV2> interactObjects = new List<InteractV2>();
    private int currentIndex;

    public bool IsBusy { get; set; }

    public IReadOnlyList<InteractV2> InteractObjects => interactObjects;

    public InteractV2 CurrentInteract
    {
        get
        {
            interactObjects.RemoveAll(item => item == null || item.gameObject == null || !item.enabled || !item.gameObject.activeInHierarchy);

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
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ForceUnlockState();
    }

    private void OnLoadingCompleteEvent(object obj)
    {
        ForceUnlockState();
    }

    /// <summary>
    /// Reset triệt để mọi trạng thái tương tác mỗi khi nạp scene hoặc đổi menu
    /// </summary>
    public void ForceUnlockState()
    {
        IsBusy = false;
        cooldownUntilTime = 0f;
        currentIndex = 0;
        interactObjects.Clear();

        if (InteractUIV2.Instance != null)
        {
            InteractUIV2.Instance.Hide();
        }

        // Tự động quét lại các Trigger xung quanh Player sau khi Scene đã load ổn định
        CancelInvoke(nameof(RescanNearbyInteracts));
        Invoke(nameof(RescanNearbyInteracts), 0.2f);
    }

    public void SetCooldown(float duration)
    {
        cooldownUntilTime = Time.unscaledTime + duration;
    }

    private void Update()
    {
        // 1. Dọn dẹp các object bị huỷ
        interactObjects.RemoveAll(item => item == null || item.gameObject == null || !item.enabled || !item.gameObject.activeInHierarchy);

        // 2. Nếu không có bảng thoại hoặc Menu nào đang mở, đảm bảo IsBusy không bị kẹt
        if (IsBusy)
        {
            bool isDialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.gameObject.activeInHierarchy && DialogueUI.Instance.transform.Find("Root") != null && DialogueUI.Instance.transform.Find("Root").gameObject.activeSelf;
            bool isMenuOpen = UIManager.Instance != null && UIManager.Instance.CurrentMenuType != MenuType.DefaultLobbyInputMenu && UIManager.Instance.CurrentMenuType != MenuType.GameplayMenu && UIManager.Instance.CurrentMenuType != MenuType.None;

            if (!isDialogueActive && !isMenuOpen)
            {
                IsBusy = false;
            }
        }

        // 3. Nếu danh sách rỗng hoặc đang bận -> Ẩn UI
        if (interactObjects.Count == 0 || IsBusy || Time.unscaledTime < cooldownUntilTime)
        {
            if (InteractUIV2.Instance != null && InteractUIV2.Instance.gameObject.activeSelf)
            {
                InteractUIV2.Instance.Hide();
            }
            return;
        }

        // 4. Lăn chuột đổi mục chọn
        if (enableMouseWheel)
        {
            HandleMouseWheel();
        }

        // 5. Bấm phím tương tác
        if (Input.GetKeyDown(interactKey))
        {
            ExecuteCurrent();
        }
    }

    /// <summary>
    /// Quét lại toàn bộ collider quanh Player để tránh tình trạng player đứng sẵn trong vùng trigger lúc load scene
    /// </summary>
    public void RescanNearbyInteracts()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Collider[] hits = Physics.OverlapSphere(player.transform.position, 3.5f);
        for (int i = 0; i < hits.Length; i++)
        {
            InteractV2 interact = hits[i].GetComponent<InteractV2>();
            if (interact != null && interact.enabled && interact.gameObject.activeInHierarchy)
            {
                Register(interact);
            }
        }
        ForceRefresh();
    }

    #region Register
    public void Register(InteractV2 interact)
    {
        if (interact == null || interactObjects.Contains(interact)) return;

        interactObjects.Add(interact);

        if (interactObjects.Count == 1)
        {
            currentIndex = 0;
        }

        RefreshUI();
        DebugCurrent();
    }

    public void Unregister(InteractV2 interact)
    {
        if (interact == null) return;

        interactObjects.Remove(interact);

        if (interactObjects.Count == 0)
        {
            currentIndex = 0;
            RefreshUI();
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, interactObjects.Count - 1);
        RefreshUI();
        DebugCurrent();
    }
    #endregion

    #region Mouse Wheel
    private void HandleMouseWheel()
    {
        if (Time.unscaledTime < nextScrollTime) return;

        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0f)
        {
            Previous();
            nextScrollTime = Time.unscaledTime + scrollCooldown;
        }
        else if (scroll < 0f)
        {
            Next();
            nextScrollTime = Time.unscaledTime + scrollCooldown;
        }
    }

    public void Next()
    {
        if (interactObjects.Count <= 1) return;

        currentIndex++;
        if (currentIndex >= interactObjects.Count)
        {
            currentIndex = 0;
        }

        RefreshUI();
        DebugCurrent();
    }

    public void Previous()
    {
        if (interactObjects.Count <= 1) return;

        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = interactObjects.Count - 1;
        }

        RefreshUI();
        DebugCurrent();
    }
    #endregion

    #region UI
    public void ForceRefresh()
    {
        interactObjects.RemoveAll(item => item == null || item.gameObject == null || !item.enabled || !item.gameObject.activeInHierarchy);

        if (interactObjects.Count == 0)
        {
            if (InteractUIV2.Instance != null) InteractUIV2.Instance.Hide();
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, interactObjects.Count - 1);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (IsBusy)
        {
            if (InteractUIV2.Instance != null)
            {
                InteractUIV2.Instance.Hide();
            }
            return;
        }

        for (int i = 0; i < interactObjects.Count; i++)
        {
            if (interactObjects[i] != null)
            {
                interactObjects[i].SetSelected(i == currentIndex);
            }
        }

        if (InteractUIV2.Instance == null) return;

        InteractUIV2.Instance.Refresh(interactObjects, currentIndex);
    }
    #endregion

    #region Debug
    private void DebugCurrent()
    {
        if (!debugMode) return;

        if (CurrentInteract == null)
        {
            Debug.Log("<color=#888888>[InteractV2] Interaction list is empty.</color>");
            return;
        }

        Debug.Log($"<color=#AAAAAA>[InteractV2] Current target focus: {CurrentInteract.InteractText}</color>");
    }
    #endregion

    #region Public API
    public bool HasInteract() => interactObjects.Count > 0;
    public int Count() => interactObjects.Count;
    public int CurrentIndex() => currentIndex;
    public List<InteractV2> GetObjects() => interactObjects;
    #endregion

    private void LateUpdate()
    {
        consumedInputThisFrame = false;
    }

    public void ExecuteCurrent()
    {
        if (IsBusy || Time.unscaledTime < cooldownUntilTime || CurrentInteract == null) return;

        consumedInputThisFrame = true;
        CurrentInteract.Execute();
    }

    public bool WasInputConsumedThisFrame() => consumedInputThisFrame;
}