using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.AchievementMenu;

    [Header("Achievement UI")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private AchievementCardUI cardPrefab;
    [SerializeField] private AchievementToastUI toastUI;

    [Header("Buttons")]
    [SerializeField] private Button btnClose;

    [Header("Optional")]
    [SerializeField] private TMP_Text txtStatus;

    [Header("Navigation")]
    [SerializeField] private MenuType fallbackMenu =
        MenuType.GameplayMenu;

    private AchievementManager achievementManager;
    private Coroutine initializeRoutine;
    private bool viewBound;

    protected override void LoadComponent()
    {
        if (achievementPanel == null)
        {
            achievementPanel =
                FindDeepChild("AchievementPanel")?.gameObject;
        }

        if (contentParent == null)
        {
            Transform scrollView =
                FindDeepChild("AchievementScrollView");

            if (scrollView != null)
            {
                Transform[] children =
                    scrollView.GetComponentsInChildren<Transform>(
                        true);

                foreach (Transform child in children)
                {
                    if (child.name == "Content")
                    {
                        contentParent = child;
                        break;
                    }
                }
            }
        }

        if (toastUI == null)
        {
            toastUI =
                GetComponentInChildren<AchievementToastUI>(true);
        }

        if (btnClose == null)
        {
            btnClose =
                FindDeepChild("CloseButton")
                ?.GetComponent<Button>();
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadComponentRuntime();

        RemoveEvents();
        AddEvents();

        SetStatus(string.Empty);

        if (achievementPanel != null)
            achievementPanel.SetActive(true);

        if (initializeRoutine != null)
            StopCoroutine(initializeRoutine);

        initializeRoutine =
            StartCoroutine(InitializeAchievementMenu());
    }

    public override void Close()
    {
        if (initializeRoutine != null)
        {
            StopCoroutine(initializeRoutine);
            initializeRoutine = null;
        }

        RemoveEvents();
        ReleaseAchievementView();

        base.Close();
    }

    private IEnumerator InitializeAchievementMenu()
    {
        float timeout = 5f;

        while (AchievementManager.Instance == null &&
               timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        initializeRoutine = null;
        achievementManager = AchievementManager.Instance;

        if (achievementManager == null)
        {
            SetStatus(
                "AchievementManager is missing. Open from Lobby.");

            yield break;
        }

        if (contentParent == null)
        {
            SetStatus(
                "Achievement Content is not assigned.");

            yield break;
        }

        if (cardPrefab == null)
        {
            SetStatus(
                "AchievementCard prefab is not assigned.");

            yield break;
        }

        achievementManager.BindUI(
            achievementPanel,
            contentParent,
            cardPrefab,
            toastUI);

        viewBound = true;

        achievementManager.OpenPanel();
    }

    private void AddEvents()
    {
        if (btnClose != null)
            btnClose.onClick.AddListener(OnCloseClicked);
    }

    private void RemoveEvents()
    {
        if (btnClose != null)
            btnClose.onClick.RemoveListener(OnCloseClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnCloseClicked();
    }

    private void OnCloseClicked()
    {
        ReleaseAchievementView();

        if (LobbyUIOverlayManager.Instance != null)
        {
            LobbyUIOverlayManager.Instance.CloseMenu();
            return;
        }

        if (UIManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        MenuType backMenu =
            UIManager.Instance.PreviousMenuType;

        if (backMenu == MenuType.None ||
            backMenu == MenuType.AchievementMenu)
        {
            backMenu = fallbackMenu;
        }

        UIManager.Instance.ChangeMenu(backMenu);
    }

    private void ReleaseAchievementView()
    {
        if (!viewBound || achievementManager == null)
            return;

        achievementManager.ClosePanel();
        achievementManager.RestoreDefaultUI();

        viewBound = false;
    }

    private void OnDestroy()
    {
        /*
         * UI Menu bi unload additive truc tiep tu Lobby,
         * nen phai tra reference UI ve manager Lobby.
         */
        ReleaseAchievementView();
    }

    private void SetStatus(string message)
    {
        if (txtStatus != null)
            txtStatus.text = message;

        if (!string.IsNullOrEmpty(message))
            Debug.LogWarning(message);
    }

    private Transform FindDeepChild(string childName)
    {
        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}