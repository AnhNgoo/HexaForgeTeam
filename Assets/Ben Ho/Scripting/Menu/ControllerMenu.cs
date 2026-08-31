using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel; // nơi chứa enum MouseButton
using UnityEngine.UI;

[Serializable]
public class KeyBindingUI
{
    public string actionName;
    public Key defaultKey;
    public Button button;
    public TMP_Text keyText;
}

public static class GameKeyBindings
{
    public static Key GetKey(string actionName, Key defaultKey)
    {
        string savedKey = PlayerPrefs.GetString(
            "Controller.Key." + actionName,
            defaultKey.ToString());

        if (Enum.TryParse(savedKey, out Key key))
            return key;

        return defaultKey;
    }

    public static MouseButton? GetMouseButton(string actionName)
    {
        string saved = PlayerPrefs.GetString(
            "Controller.MouseButton." + actionName,
            string.Empty);

        if (Enum.TryParse(saved, out MouseButton button))
            return button;

        return null;
    }

    public static bool WasPressedThisFrame(string actionName, Key defaultKey)
    {
        MouseButton? mouseButton = GetMouseButton(actionName);

        if (mouseButton.HasValue)
        {
            if (Mouse.current == null)
                return false;

            switch (mouseButton.Value)
            {
                case MouseButton.Left:    return Mouse.current.leftButton.wasPressedThisFrame;
                case MouseButton.Right:   return Mouse.current.rightButton.wasPressedThisFrame;
                case MouseButton.Middle:  return Mouse.current.middleButton.wasPressedThisFrame;
                case MouseButton.Back:    return Mouse.current.backButton.wasPressedThisFrame;
                case MouseButton.Forward: return Mouse.current.forwardButton.wasPressedThisFrame;
            }

            return false;
        }

        if (Keyboard.current == null)
            return false;

        Key key = GetKey(actionName, defaultKey);

        try
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }
        catch
        {
            return false;
        }
    }

    public static string GetMouseButtonPath(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:    return "<Mouse>/leftButton";
            case MouseButton.Right:   return "<Mouse>/rightButton";
            case MouseButton.Middle:  return "<Mouse>/middleButton";
            case MouseButton.Back:    return "<Mouse>/backButton";
            case MouseButton.Forward: return "<Mouse>/forwardButton";
            default:                  return string.Empty;
        }
    }
}

public class ControllerMenu : MenuBase
{
    public override MenuType menuType => MenuType.ControllerMenu;

    [Header("Setting Tabs")]
    [SerializeField] private SettingsTabUI tabs;

    [Header("Controller Controls")]
    [SerializeField] private Slider sliderHorizontalSensitivity;
    [SerializeField] private Slider sliderVerticalSensitivity;
    [SerializeField] private Toggle toggleVibration;
    [SerializeField] private Toggle toggleAimAssist;

    [Header("Control Type")]
    [SerializeField] private ArrowSelectorUI controlTypeSelector;
    [SerializeField] private string[] controlTypes = { "Keyboard & Mouse", "Controller" };

    [Header("Key Bindings")]
    [SerializeField] private KeyBindingUI[] keyBindings;

    [Header("Buttons")]
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnBack;

    [Header("Embedded System Settings")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    [Header("Scroll Panel")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollStep = 0.12f;

    private bool eventsAdded;
    private int controlTypeIndex;
    private int rebindingIndex = -1;
    private int rebindStartFrame = -1;
    private int lastAssignedIndex = -1;
    private int lastAssignedFrame = -1;

    private Key[] currentKeys;
    private MouseButton?[] currentMouseButtons;
    private UnityAction[] keyButtonActions;

    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }

    public override void Open(object data = null)
    {
        base.Open(data);
        Initialize();
    }

    public override void Close()
    {
        rebindingIndex = -1;
        RemoveEvents();
        base.Close();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        rebindingIndex = -1;
        RemoveEvents();
    }

    private void Update()
    {
        UpdateManualScroll();

        // KHÔNG ở chế độ rebind
        if (rebindingIndex < 0)
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                ConfirmAndBack();
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Back();
                return;
            }

            return;
        }

        // Bỏ qua frame vừa bắt đầu rebind
        if (Time.frameCount <= rebindStartFrame)
            return;

        // --- REBINDING ---
        PressedInput pressed = GetPressedInput();

        if (pressed.type == PressedInputType.None)
            return;

        // ESC = hủy gán
        if (pressed.type == PressedInputType.Keyboard && pressed.key == Key.Escape)
        {
            rebindingIndex = -1;
            RefreshKeyTexts();
            return;
        }

        // Click chuột rơi vào NÚT UI KHÁC (dòng khác, tab, confirm, slider...) => không gán,
        // chỉ để hủy dòng cũ / chuyển sang dòng mới.
        if (pressed.type == PressedInputType.Mouse && IsPointerOverOtherUISelectable(rebindingIndex))
            return;

        if (pressed.type == PressedInputType.Keyboard)
        {
            currentKeys[rebindingIndex] = pressed.key;
            currentMouseButtons[rebindingIndex] = null;
        }
        else
        {
            // Click trên chính dòng đang gán hoặc vùng trống => gán nút chuột
            currentMouseButtons[rebindingIndex] = pressed.mouseButton;
            currentKeys[rebindingIndex] = Key.None;
        }

        lastAssignedIndex = rebindingIndex;
        lastAssignedFrame = Time.frameCount;

        rebindingIndex = -1;
        RefreshKeyTexts();
        SaveKeyBindingsNow(); // lưu NGAY khi gán xong -> không bao giờ bị revert
    }

    private void Initialize()
    {
        RemoveEvents();
        EnsureRuntimeKeyBindings();
        EnsureScrollRect();
        LoadSettings();
        AddEvents();
        tabs?.SetSelected(MenuType.ControllerMenu);
    }

    #region EVENTS

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        AddButton(controlTypeSelector.leftButton, PreviousControlType);
        AddButton(controlTypeSelector.rightButton, NextControlType);

        AddButton(tabs?.btnAudio, OpenAudioTab);
        AddButton(tabs?.btnGraphics, OpenGraphicsTab);
        AddButton(tabs?.btnController, OpenControllerTab);

        AddButton(btnConfirm, Confirm);
        AddButton(btnBack, Back);

        if (keyBindings != null)
        {
            keyButtonActions = new UnityAction[keyBindings.Length];

            for (int i = 0; i < keyBindings.Length; i++)
            {
                if (keyBindings[i] == null || keyBindings[i].button == null)
                    continue;

                int index = i;
                keyButtonActions[i] = () => BeginRebind(index);
                keyBindings[i].button.onClick.AddListener(keyButtonActions[i]);
            }
        }

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        RemoveButton(controlTypeSelector.leftButton, PreviousControlType);
        RemoveButton(controlTypeSelector.rightButton, NextControlType);

        RemoveButton(tabs?.btnAudio, OpenAudioTab);
        RemoveButton(tabs?.btnGraphics, OpenGraphicsTab);
        RemoveButton(tabs?.btnController, OpenControllerTab);

        RemoveButton(btnConfirm, Confirm);
        RemoveButton(btnBack, Back);

        if (keyBindings != null && keyButtonActions != null)
        {
            int count = Mathf.Min(keyBindings.Length, keyButtonActions.Length);

            for (int i = 0; i < count; i++)
            {
                if (keyBindings[i] != null &&
                    keyBindings[i].button != null &&
                    keyButtonActions[i] != null)
                {
                    keyBindings[i].button.onClick.RemoveListener(keyButtonActions[i]);
                }
            }
        }

        keyButtonActions = null;
        eventsAdded = false;
    }

    #endregion

    #region LOAD / SAVE

    private void LoadSettings()
    {
        controlTypeIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Controller.ControlType", 0),
            0,
            Mathf.Max(0, controlTypes.Length - 1));

        SetSlider(sliderHorizontalSensitivity, PlayerPrefs.GetFloat("Controller.HorizontalSensitivity", 1f));
        SetSlider(sliderVerticalSensitivity, PlayerPrefs.GetFloat("Controller.VerticalSensitivity", 1f));
        SetToggle(toggleVibration, PlayerPrefs.GetInt("Controller.Vibration", 1) == 1);
        SetToggle(toggleAimAssist, PlayerPrefs.GetInt("Controller.AimAssist", 1) == 1);

        LoadKeyBindings();
        RefreshSelectors();
    }

    private void LoadKeyBindings()
    {
        if (keyBindings == null)
            return;

        currentKeys = new Key[keyBindings.Length];
        currentMouseButtons = new MouseButton?[keyBindings.Length];

        for (int i = 0; i < keyBindings.Length; i++)
        {
            KeyBindingUI binding = keyBindings[i];

            if (binding == null)
                continue;

            currentKeys[i] = GameKeyBindings.GetKey(
                binding.actionName,
                GetInputDefaultKey(binding.actionName, binding.defaultKey));

            currentMouseButtons[i] = GameKeyBindings.GetMouseButton(binding.actionName);
        }

        RefreshKeyTexts();
    }

    // LƯU NGAY LẬP TỨC mỗi khi gán xong -> không bị trả về giá trị cũ nữa
    private void SaveKeyBindingsNow()
    {
        if (keyBindings == null || currentKeys == null || currentMouseButtons == null)
            return;

        int count = Mathf.Min(keyBindings.Length, currentKeys.Length);

        for (int i = 0; i < count; i++)
        {
            if (keyBindings[i] == null || string.IsNullOrWhiteSpace(keyBindings[i].actionName))
                continue;

            PlayerPrefs.SetString(
                "Controller.Key." + keyBindings[i].actionName,
                currentKeys[i].ToString());

            PlayerPrefs.SetString(
                "Controller.MouseButton." + keyBindings[i].actionName,
                currentMouseButtons[i].HasValue
                    ? currentMouseButtons[i].Value.ToString()
                    : string.Empty);
        }

        ApplyInputActionOverrides();
        PlayerPrefs.Save();
    }

    private void Confirm()
    {
        PlayerPrefs.SetInt("Controller.ControlType", controlTypeIndex);

        SaveSlider("Controller.HorizontalSensitivity", sliderHorizontalSensitivity);
        SaveSlider("Controller.VerticalSensitivity", sliderVerticalSensitivity);

        SaveToggle("Controller.Vibration", toggleVibration);
        SaveToggle("Controller.AimAssist", toggleAimAssist);

        SaveKeyBindingsNow();
    }

    private void ApplyInputActionOverrides()
    {
        if (keyBindings == null || currentKeys == null || currentMouseButtons == null)
            return;

        InputActions inputActions = InputManager.InputActions;
        bool ownsInputActions = inputActions == null;

        if (ownsInputActions)
        {
            inputActions = new InputActions();

            string savedOverrides = PlayerPrefs.GetString(
                InputManager.BindingOverridesPlayerPrefsKey,
                string.Empty);

            if (!string.IsNullOrEmpty(savedOverrides))
                inputActions.LoadBindingOverridesFromJson(savedOverrides);
        }

        inputActions.Disable();

        try
        {
            int count = Mathf.Min(keyBindings.Length, currentKeys.Length);

            for (int i = 0; i < count; i++)
            {
                KeyBindingUI bindingUI = keyBindings[i];

                if (bindingUI == null || string.IsNullOrWhiteSpace(bindingUI.actionName))
                    continue;

                string path;

                if (currentMouseButtons[i].HasValue)
                    path = GameKeyBindings.GetMouseButtonPath(currentMouseButtons[i].Value);
                else if (currentKeys[i] != Key.None && Keyboard.current != null)
                    path = Keyboard.current[currentKeys[i]].path;
                else
                    continue;

                if (string.IsNullOrEmpty(path))
                    continue;

                ResolveInputAction(
                    bindingUI.actionName,
                    out string inputActionName,
                    out string compositePartName);

                InputAction action = inputActions.asset.FindAction(
                    "Keyboard/" + inputActionName,
                    false);

                if (action == null)
                {
                    Debug.LogWarning($"ControllerMenu: Input Action '{inputActionName}' was not found.");
                    continue;
                }

                int bindingIndex = FindKeyboardBindingIndex(action, compositePartName);

                if (bindingIndex < 0)
                {
                    Debug.LogWarning(
                        $"ControllerMenu: Keyboard binding for '{bindingUI.actionName}' was not found.");
                    continue;
                }

                action.ApplyBindingOverride(bindingIndex, path);
            }

            PlayerPrefs.SetString(
                InputManager.BindingOverridesPlayerPrefsKey,
                inputActions.SaveBindingOverridesAsJson());
        }
        finally
        {
            if (ownsInputActions)
                inputActions.Dispose();
            else
                inputActions.Enable();
        }
    }

    private static int FindKeyboardBindingIndex(InputAction action, string compositePartName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (!string.IsNullOrEmpty(compositePartName))
            {
                if (binding.isPartOfComposite &&
                    string.Equals(binding.name, compositePartName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                continue;
            }

            bool isKeyboardOrMousePath =
                binding.path.StartsWith("<Keyboard>", StringComparison.OrdinalIgnoreCase) ||
                binding.path.StartsWith("<Mouse>", StringComparison.OrdinalIgnoreCase);

            if (!binding.isComposite && !binding.isPartOfComposite && isKeyboardOrMousePath)
                return i;
        }

        return -1;
    }

    private static void ResolveInputAction(
        string menuActionName,
        out string inputActionName,
        out string compositePartName)
    {
        compositePartName = string.Empty;

        switch (menuActionName.Trim())
        {
            case "Move Forward":  inputActionName = "Move"; compositePartName = "up";    break;
            case "Move Backward": inputActionName = "Move"; compositePartName = "down";  break;
            case "Move Left":     inputActionName = "Move"; compositePartName = "left";  break;
            case "Move Right":    inputActionName = "Move"; compositePartName = "right"; break;
            case "Sneak":
            case "Sneak/Crouch":
            case "Sneak / Crouch": inputActionName = "Walk"; break;
            default: inputActionName = menuActionName.Trim(); break;
        }
    }

    private static Key GetInputDefaultKey(string menuActionName, Key fallback)
    {
        switch (menuActionName.Trim())
        {
            case "Move Forward":  return Key.W;
            case "Move Backward": return Key.S;
            case "Move Left":     return Key.A;
            case "Move Right":    return Key.D;
            case "Jump":          return Key.Space;
            case "Dodge":         return Key.LeftShift;
            case "Sprint":        return Key.LeftAlt;
            case "Sneak":
            case "Sneak/Crouch":
            case "Sneak / Crouch": return Key.LeftCtrl;
            case "Interact":      return Key.F;
            default:              return fallback;
        }
    }

    #endregion

    #region RUNTIME KEY BINDINGS

    private void EnsureRuntimeKeyBindings()
    {
        if (keyBindings != null && keyBindings.Length > 0)
            return;

        RuntimeBindingSpec[] specs =
        {
            new RuntimeBindingSpec("Jump", Key.Space),
            new RuntimeBindingSpec("Dodge", Key.LeftShift),
            new RuntimeBindingSpec("Sprint", Key.LeftAlt),
            new RuntimeBindingSpec("Sneak/Crouch", Key.LeftCtrl, "Sneak / Crouch", "Sneak"),
            new RuntimeBindingSpec("Interact", Key.F),
            new RuntimeBindingSpec("Move Forward", Key.W),
            new RuntimeBindingSpec("Move Backward", Key.S),
            new RuntimeBindingSpec("Move Right", Key.D),
            new RuntimeBindingSpec("Move Left", Key.A)
        };

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        List<KeyBindingUI> bindings = new List<KeyBindingUI>(specs.Length);

        foreach (RuntimeBindingSpec spec in specs)
        {
            TMP_Text titleText = FindText(texts, spec);

            if (titleText == null || titleText.transform.parent == null)
                continue;

            Transform row = titleText.transform.parent;
            TMP_Text keyText = FindKeyText(row, titleText);

            if (keyText == null)
                continue;

            Transform clickTarget = keyText.transform.parent != null
                ? keyText.transform.parent
                : keyText.transform;

            Button button = clickTarget.GetComponent<Button>();

            if (button == null)
                button = clickTarget.GetComponentInParent<Button>();

            if (button == null)
            {
                button = clickTarget.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = keyText;
            }

            bindings.Add(new KeyBindingUI
            {
                actionName = spec.ActionName,
                defaultKey = spec.DefaultKey,
                button = button,
                keyText = keyText
            });
        }

        keyBindings = bindings.ToArray();
    }

    private static TMP_Text FindText(TMP_Text[] texts, RuntimeBindingSpec spec)
    {
        foreach (TMP_Text text in texts)
        {
            if (text == null)
                continue;

            string value = text.text.Trim();

            if (string.Equals(value, spec.ActionName, StringComparison.OrdinalIgnoreCase))
                return text;

            foreach (string alias in spec.Aliases)
            {
                if (string.Equals(value, alias, StringComparison.OrdinalIgnoreCase))
                    return text;
            }
        }

        return null;
    }

    private static TMP_Text FindKeyText(Transform row, TMP_Text titleText)
    {
        TMP_Text[] rowTexts = row.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in rowTexts)
        {
            if (text != null && text != titleText)
                return text;
        }

        return null;
    }

    private readonly struct RuntimeBindingSpec
    {
        public readonly string ActionName;
        public readonly Key DefaultKey;
        public readonly string[] Aliases;

        public RuntimeBindingSpec(string actionName, Key defaultKey, params string[] aliases)
        {
            ActionName = actionName;
            DefaultKey = defaultKey;
            Aliases = aliases ?? Array.Empty<string>();
        }
    }

    #endregion

    #region SCROLL PANEL

    private void EnsureScrollRect()
    {
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>(true);

        if (scrollRect == null)
            scrollRect = GetComponentInParent<ScrollRect>(true);

        if (scrollRect == null)
        {
            ScrollRect[] all = FindObjectsOfType<ScrollRect>(true);
            ScrollRect fallback = null;

            for (int i = 0; i < all.Length; i++)
            {
                ScrollRect sr = all[i];

                if (sr == null || !sr.gameObject.activeInHierarchy)
                    continue;

                if (keyBindings != null &&
                    keyBindings.Length > 0 &&
                    keyBindings[0] != null &&
                    keyBindings[0].button != null &&
                    keyBindings[0].button.transform.IsChildOf(sr.transform))
                {
                    scrollRect = sr;
                    break;
                }

                if (fallback == null)
                    fallback = sr;
            }

            if (scrollRect == null)
                scrollRect = fallback;
        }

        if (scrollRect != null && scrollRect.content == null &&
            keyBindings != null && keyBindings.Length > 0 &&
            keyBindings[0] != null && keyBindings[0].button != null)
        {
            Transform row = keyBindings[0].button.transform;

            if (row.parent != null)
                scrollRect.content = row.parent as RectTransform;
        }

        if (scrollRect != null && scrollRect.viewport == null)
        {
            scrollRect.viewport = scrollRect.content != null
                ? scrollRect.content.parent as RectTransform
                : scrollRect.transform as RectTransform;
        }

        if (scrollRect == null)
        {
            Debug.LogWarning(
                "ControllerMenu: Không tìm thấy ScrollRect. " +
                "Kéo object có ScrollRect vào ô 'Scroll Rect' trong Inspector.");
            return;
        }

        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        scrollRect.enabled = true;
        scrollRect.scrollSensitivity = 0f;

        if (scrollRect.viewport != null &&
            scrollRect.viewport.GetComponent<RectMask2D>() == null &&
            scrollRect.viewport.GetComponent<Mask>() == null)
        {
            scrollRect.viewport.gameObject.AddComponent<RectMask2D>();
        }
    }

    private void UpdateManualScroll()
    {
        if (scrollRect == null || scrollRect.content == null)
            return;

        if (Mouse.current == null)
            return;

        float wheel = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(wheel, 0f))
            return;

        RectTransform viewport = scrollRect.viewport != null
            ? scrollRect.viewport
            : scrollRect.transform as RectTransform;

        if (viewport == null || !IsPointerOverRect(viewport))
            return;

        float step = Mathf.Clamp(wheel / 120f, -1f, 1f) * scrollStep;

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            scrollRect.verticalNormalizedPosition + step);
    }

    private bool IsPointerOverRect(RectTransform rect)
    {
        if (rect == null || Mouse.current == null)
            return false;

        Vector2 pos = Mouse.current.position.ReadValue();

        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera cam = null;

        if (canvas != null)
        {
            Canvas root = canvas.rootCanvas;

            if (root != null && root.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = root.worldCamera;
        }

        if (cam != null && RectTransformUtility.RectangleContainsScreenPoint(rect, pos, cam))
            return true;

        return RectTransformUtility.RectangleContainsScreenPoint(rect, pos, null);
    }

    #endregion

    #region REBIND

    public void BeginRebind(int index)
    {
        if (currentKeys == null ||
            keyBindings == null ||
            index < 0 ||
            index >= keyBindings.Length)
            return;

        // Chặn onClick bắn lại ngay sau khi vừa gán xong (click cùng dòng để gán chuột)
        if (index == lastAssignedIndex && Time.frameCount <= lastAssignedFrame + 2)
            return;

        // QUAN TRỌNG: bỏ select để Space/Enter KHÔNG bị Unity UI hiểu là "click" lên dòng đó
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Dòng đang chờ trước đó => trả chữ về cho nó
        if (rebindingIndex >= 0 &&
            rebindingIndex != index &&
            rebindingIndex < keyBindings.Length)
        {
            RefreshKeyText(rebindingIndex);
        }

        rebindingIndex = index;
        rebindStartFrame = Time.frameCount;

        if (keyBindings[index] != null && keyBindings[index].keyText != null)
            keyBindings[index].keyText.text = "...";
    }

    // Click chuột có đang rơi vào nút UI KHÁC (không phải dòng đang gán) không?
    private bool IsPointerOverOtherUISelectable(int currentIndex)
    {
        EventSystem es = EventSystem.current;

        if (es == null || Mouse.current == null)
            return false;

        PointerEventData data = new PointerEventData(es)
        {
            position = Mouse.current.position.ReadValue()
        };

        raycastResults.Clear();
        es.RaycastAll(data, raycastResults);

        Transform currentRow = (keyBindings != null &&
                                currentIndex >= 0 &&
                                currentIndex < keyBindings.Length &&
                                keyBindings[currentIndex] != null &&
                                keyBindings[currentIndex].button != null)
            ? keyBindings[currentIndex].button.transform
            : null;

        for (int i = 0; i < raycastResults.Count; i++)
        {
            Selectable sel = raycastResults[i].gameObject.GetComponentInParent<Selectable>();

            if (sel == null)
                continue;

            // Nằm trên CHÍNH dòng đang gán => cho phép gán nút chuột
            if (currentRow != null &&
                (sel.transform == currentRow ||
                 sel.transform.IsChildOf(currentRow) ||
                 currentRow.IsChildOf(sel.transform)))
                continue;

            return true; // nút UI khác => không gán
        }

        return false;
    }

    private enum PressedInputType
    {
        None,
        Keyboard,
        Mouse
    }

    private struct PressedInput
    {
        public PressedInputType type;
        public Key key;
        public MouseButton mouseButton;
    }

    private static PressedInput GetPressedInput()
    {
        if (Keyboard.current != null)
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key == Key.None)
                    continue;

                try
                {
                    if (Keyboard.current[key].wasPressedThisFrame)
                        return new PressedInput { type = PressedInputType.Keyboard, key = key };
                }
                catch
                {
                    // enum value không hợp lệ
                }
            }
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                return new PressedInput { type = PressedInputType.Mouse, mouseButton = MouseButton.Left };

            if (Mouse.current.rightButton.wasPressedThisFrame)
                return new PressedInput { type = PressedInputType.Mouse, mouseButton = MouseButton.Right };

            if (Mouse.current.middleButton.wasPressedThisFrame)
                return new PressedInput { type = PressedInputType.Mouse, mouseButton = MouseButton.Middle };

            if (Mouse.current.backButton.wasPressedThisFrame)
                return new PressedInput { type = PressedInputType.Mouse, mouseButton = MouseButton.Back };

            if (Mouse.current.forwardButton.wasPressedThisFrame)
                return new PressedInput { type = PressedInputType.Mouse, mouseButton = MouseButton.Forward };
        }

        return new PressedInput { type = PressedInputType.None };
    }

    #endregion

    #region UI HELPERS

    private void OpenAudioTab()      => OpenSettingTab(MenuType.SettingMenu, SystemSettingPage.Audio);
    private void OpenGraphicsTab()   => OpenSettingTab(MenuType.GraphicsMenu, SystemSettingPage.Graphics);
    private void OpenControllerTab() => OpenSettingTab(MenuType.ControllerMenu, SystemSettingPage.Controller);

    private void OpenSettingTab(MenuType targetMenu, SystemSettingPage systemPage)
    {
        if (systemSettingsPanel != null)
        {
            systemSettingsPanel.ShowPage(systemPage);
            return;
        }

        if (targetMenu == menuType)
        {
            tabs?.SetSelected(menuType);
            return;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(targetMenu);
    }

    private void Back()
    {
        LoadSettings();

        if (systemSettingsPanel != null)
            systemSettingsPanel.CloseGameSystemMenu();
        else if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(SettingMenuData.BackMenu);
    }

    private void PreviousControlType()
    {
        controlTypeIndex = Wrap(controlTypeIndex - 1, controlTypes.Length);
        RefreshSelectors();
    }

    private void NextControlType()
    {
        controlTypeIndex = Wrap(controlTypeIndex + 1, controlTypes.Length);
        RefreshSelectors();
    }

    private void RefreshSelectors()
    {
        if (controlTypes == null || controlTypes.Length == 0)
        {
            controlTypeSelector.SetText("");
            return;
        }

        controlTypeSelector.SetText(
            controlTypes[Mathf.Clamp(controlTypeIndex, 0, controlTypes.Length - 1)]);
    }

    private void RefreshKeyTexts()
    {
        if (keyBindings == null || currentKeys == null)
            return;

        int count = Mathf.Min(keyBindings.Length, currentKeys.Length);

        for (int i = 0; i < count; i++)
        {
            if (i == rebindingIndex)
                continue;

            RefreshKeyText(i);
        }
    }

        private void LateUpdate()
    {
        EnforceKeyTexts();
    }

    // MỚI: "khóa" chữ hiển thị của các dòng key binding.
    // Nếu có script khác (AutoTranslateUI, refresh icon...) ghi đè lên text,
    // ta sửa lại ngay trong cùng frame -> hết hiện tượng tự nhảy về Space.
    private void EnforceKeyTexts()
    {
        if (keyBindings == null || currentKeys == null)
            return;

        int count = Mathf.Min(keyBindings.Length, currentKeys.Length);

        for (int i = 0; i < count; i++)
        {
            // dòng đang chờ gán phím thì giữ chữ "..."
            if (i == rebindingIndex)
                continue;

            KeyBindingUI binding = keyBindings[i];

            if (binding == null || binding.keyText == null)
                continue;

            string expected;

            if (currentMouseButtons != null && currentMouseButtons[i].HasValue)
                expected = FormatMouseButton(currentMouseButtons[i].Value);
            else if (currentKeys[i] != Key.None)
                expected = FormatKey(currentKeys[i]);
            else
                expected = "None";

            if (binding.keyText.text != expected)
                binding.keyText.text = expected;
        }
    }

    private void RefreshKeyText(int index)
    {
        if (keyBindings == null || index < 0 || index >= keyBindings.Length)
            return;

        KeyBindingUI binding = keyBindings[index];

        if (binding == null || binding.keyText == null)
            return;

        if (currentMouseButtons != null && currentMouseButtons[index].HasValue)
            binding.keyText.text = FormatMouseButton(currentMouseButtons[index].Value);
        else if (currentKeys != null && currentKeys[index] != Key.None)
            binding.keyText.text = FormatKey(currentKeys[index]);
        else
            binding.keyText.text = "None";
    }

    private string FormatKey(Key key)
    {
        switch (key)
        {
            case Key.Space:      return "Space";
            case Key.LeftShift:
            case Key.RightShift: return "Shift";
            case Key.LeftCtrl:
            case Key.RightCtrl:  return "Ctrl";
            default:             return key.ToString();
        }
    }

    private string FormatMouseButton(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:    return "Left Click";
            case MouseButton.Right:   return "Right Click";
            case MouseButton.Middle:  return "Middle Click";
            case MouseButton.Back:    return "Mouse Back";
            case MouseButton.Forward: return "Mouse Forward";
            default:                  return button.ToString();
        }
    }

    private int Wrap(int index, int length)
    {
        if (length <= 0) return 0;
        if (index < 0) return length - 1;
        if (index >= length) return 0;
        return index;
    }

    private void AddButton(Button button, UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    private void RemoveButton(Button button, UnityAction action)
    {
        if (button != null)
            button.onClick.RemoveListener(action);
    }

    private void SetSlider(Slider slider, float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void SaveSlider(string key, Slider slider)
    {
        if (slider != null)
            PlayerPrefs.SetFloat(key, slider.value);
    }

    private void SetToggle(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    private void SaveToggle(string key, Toggle toggle)
    {
        if (toggle != null)
            PlayerPrefs.SetInt(key, toggle.isOn ? 1 : 0);
    }

    private void ConfirmAndBack()
    {
        Confirm();

        if (systemSettingsPanel != null)
            systemSettingsPanel.CloseGameSystemMenu();
        else if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(SettingMenuData.BackMenu);
    }

    #endregion
}