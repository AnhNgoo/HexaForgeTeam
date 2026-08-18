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
            interactObjects.RemoveAll(item => item == null || item.gameObject == null);

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

        EventManager.Subscribe(GameEvent.OnLoadingComplete, OnLoadingCompleteEvent);
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
        EventManager.Unsubscribe(GameEvent.OnLoadingComplete, OnLoadingCompleteEvent);
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
    /// Reset triệt để mọi trạng thái khóa tương tác, cooldown và dọn sạch danh sách rác khi chuyển Scene
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

        // Tự động kiểm tra và làm mới lại UI sau 1 khoảng trễ ngắn để nhận diện các NPC ở Scene mới
        CancelInvoke(nameof(ForceRefresh));
        Invoke(nameof(ForceRefresh), 0.15f);
    }

    public void SetCooldown(float duration)
    {
        cooldownUntilTime = Time.unscaledTime + duration;
    }

    private void Update()
    {
        // Loại bỏ triệt để các Object đã bị Destroy hoặc NULL khỏi danh sách đăng ký
        interactObjects.RemoveAll(item => item == null || item.gameObject == null);

        // Nếu danh sách trống, giải phóng IsBusy và ẩn UI lập tức
        if (interactObjects.Count == 0)
        {
            if (IsBusy && (DialogueUI.Instance == null || !DialogueUI.Instance.gameObject.activeInHierarchy))
            {
                IsBusy = false;
            }

            if (InteractUIV2.Instance != null && InteractUIV2.Instance.gameObject.activeSelf)
            {
                InteractUIV2.Instance.Hide();
            }
            return;
        }

        if (IsBusy || Time.unscaledTime < cooldownUntilTime)
        {
            if (InteractUIV2.Instance != null && InteractUIV2.Instance.gameObject.activeSelf)
            {
                InteractUIV2.Instance.Hide();
            }
            return;
        }

        if (enableMouseWheel)
        {
            HandleMouseWheel();
        }

        if (Input.GetKeyDown(interactKey))
        {
            ExecuteCurrent();
        }
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
        interactObjects.RemoveAll(item => item == null || item.gameObject == null);

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