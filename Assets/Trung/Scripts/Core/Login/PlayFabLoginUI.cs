using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayFabLoginUI : MonoBehaviour
{
    public static PlayFabLoginUI Instance;

    [Header("Tab Header Buttons")]
    [SerializeField] private Button loginTabButton;
    [SerializeField] private Button registerTabButton;
    [SerializeField] private TMP_Text loginTabText;
    [SerializeField] private TMP_Text registerTabText;
    [SerializeField] private GameObject loginSelectedLine;
    [SerializeField] private GameObject registerSelectedLine;

    [Header("Tab Visual Config")]
    [SerializeField] private Color activeTabColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color inactiveTabColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private float activeTabScale = 1.15f;
    [SerializeField] private float inactiveTabScale = 1.0f;

    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;

    [Header("Action Buttons")]
    [SerializeField] private Button loginActionButton;
    [SerializeField] private Button registerActionButton;

    [Header("Password Eye Toggle")]
    [SerializeField] private Button eyeButton;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Image eyeIconImage;
    [SerializeField] private Sprite eyeShowSprite;
    [SerializeField] private Sprite eyeHideSprite;

    [Header("Input Navigation")]
    [SerializeField] private TMP_InputField loginUsernameInput;

    [Header("UI Tooltips setup (Optional UI)")]
    [SerializeField] private Toggle rememberToggle;
    [SerializeField] private TMP_Dropdown savedAccountDropdown;

    private bool isPasswordVisible = false;
    private bool isLoginTabActive = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (loginTabButton != null)
        {
            loginTabButton.onClick.RemoveAllListeners();
            loginTabButton.onClick.AddListener(() => SwitchTab(true));
        }

        if (registerTabButton != null)
        {
            registerTabButton.onClick.RemoveAllListeners();
            registerTabButton.onClick.AddListener(() => SwitchTab(false));
        }

        if (eyeButton != null)
        {
            eyeButton.onClick.RemoveAllListeners();
            eyeButton.onClick.AddListener(() =>
            {
                AnimateButtonPunch(eyeButton.transform);
                TogglePasswordVisibility();
            });
        }

        if (loginActionButton != null)
        {
            loginActionButton.onClick.RemoveAllListeners();
            loginActionButton.onClick.AddListener(() =>
            {
                AnimateButtonPunch(loginActionButton.transform);
                if (PlayFabLoginManager.Instance != null) PlayFabLoginManager.Instance.Login();
            });
        }

        if (registerActionButton != null)
        {
            registerActionButton.onClick.RemoveAllListeners();
            registerActionButton.onClick.AddListener(() =>
            {
                AnimateButtonPunch(registerActionButton.transform);
                if (PlayFabLoginManager.Instance != null) PlayFabLoginManager.Instance.Register();
            });
        }

        SetupTooltips();
        SwitchTab(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (loginUsernameInput != null && loginUsernameInput.isFocused && loginPasswordInput != null)
            {
                loginPasswordInput.Select();
            }
            else if (loginPasswordInput != null && loginPasswordInput.isFocused && loginUsernameInput != null)
            {
                loginUsernameInput.Select();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isLoginTabActive && loginActionButton != null && loginActionButton.interactable)
            {
                AnimateButtonPunch(loginActionButton.transform);
                loginActionButton.onClick.Invoke();
            }
            else if (!isLoginTabActive && registerActionButton != null && registerActionButton.interactable)
            {
                AnimateButtonPunch(registerActionButton.transform);
                registerActionButton.onClick.Invoke();
            }
        }
    }

    public void SwitchTab(bool showLogin)
    {
        isLoginTabActive = showLogin;

        // 1. Reset Scale & Kill Tween cu tren Nut Tab
        if (loginTabButton != null)
        {
            loginTabButton.transform.DOKill(true);
            loginTabButton.transform.localScale = Vector3.one;
        }

        if (registerTabButton != null)
        {
            registerTabButton.transform.DOKill(true);
            registerTabButton.transform.localScale = Vector3.one;
        }

        // 2. Bat/An Panel
        if (loginPanel != null) loginPanel.SetActive(showLogin);
        if (registerPanel != null) registerPanel.SetActive(!showLogin);

        if (loginSelectedLine != null) loginSelectedLine.SetActive(showLogin);
        if (registerSelectedLine != null) registerSelectedLine.SetActive(!showLogin);

        // 3. Doi Mau va Phongs to/Thu nho Text Tab mượt mà
        if (loginTabText != null)
        {
            loginTabText.DOKill();
            loginTabText.color = showLogin ? activeTabColor : inactiveTabColor;
            float targetScale = showLogin ? activeTabScale : inactiveTabScale;
            loginTabText.transform.DOScale(Vector3.one * targetScale, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        if (registerTabText != null)
        {
            registerTabText.DOKill();
            registerTabText.color = !showLogin ? activeTabColor : inactiveTabColor;
            float targetScale = !showLogin ? activeTabScale : inactiveTabScale;
            registerTabText.transform.DOScale(Vector3.one * targetScale, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        // 4. Animate Thanh Gach Chan (Selected Line)
        GameObject activeLine = showLogin ? loginSelectedLine : registerSelectedLine;
        if (activeLine != null)
        {
            activeLine.transform.DOKill(true);
            activeLine.transform.localScale = new Vector3(0f, 1f, 1f);
            activeLine.transform.DOScaleX(1f, 0.25f).SetEase(Ease.OutCubic).SetUpdate(true);
        }
    }

    public void TogglePasswordVisibility()
    {
        if (loginPasswordInput == null) return;

        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            loginPasswordInput.contentType = TMP_InputField.ContentType.Standard;
            if (eyeIconImage != null && eyeHideSprite != null) eyeIconImage.sprite = eyeHideSprite;
        }
        else
        {
            loginPasswordInput.contentType = TMP_InputField.ContentType.Password;
            if (eyeIconImage != null && eyeShowSprite != null) eyeIconImage.sprite = eyeShowSprite;
        }

        loginPasswordInput.ForceLabelUpdate();
    }

    private void AnimateButtonPunch(Transform btnTransform)
    {
        if (btnTransform == null) return;
        btnTransform.DOKill(true);
        btnTransform.localScale = Vector3.one;
        btnTransform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.2f, 6, 0.5f).SetUpdate(true);
    }

    private void SetupTooltips()
    {
        AddTooltipToObj(eyeButton != null ? eyeButton.gameObject : null, "Show/Hide Password", "Toggle password character visibility.");
        AddTooltipToObj(rememberToggle != null ? rememberToggle.gameObject : null, "Remember Account", "Save login credentials for seamless auto-connect next time.");
        AddTooltipToObj(savedAccountDropdown != null ? savedAccountDropdown.gameObject : null, "Account Vault", "Quickly select previously saved accounts on this device.");
        AddTooltipToObj(loginActionButton != null ? loginActionButton.gameObject : null, "Sign In", "Authenticate and connect to the realm.");
        AddTooltipToObj(registerActionButton != null ? registerActionButton.gameObject : null, "Create Account", "Register a new adventurer account.");
    }

    private void AddTooltipToObj(GameObject targetObj, string title, string desc)
    {
        if (targetObj == null) return;
        var trigger = targetObj.GetComponent<UITooltipAutoTrigger>() ?? targetObj.AddComponent<UITooltipAutoTrigger>();
        trigger.SetData(title, desc);
    }
}