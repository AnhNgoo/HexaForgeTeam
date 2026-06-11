// using System;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEditor.SceneManagement;
// #endif

// public class MenuUI : MonoBehaviour
// {
//     private const string LanguagePrefKey = "LANGUAGE_CODE";
//     private static readonly HashSet<int> BoundInstanceIds = new HashSet<int>();
//     private static readonly List<ClickTarget> ClickTargets = new List<ClickTarget>();
//     private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>();
//     private static MenuUIRuntimeInput s_runtimeInput;

//     private static readonly Dictionary<string, string> SceneMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
//     {
//         { "Play", "UI Gameplay" },
//         { "Game", "UI Gameplay" },
//         { "Gameplay", "UI Gameplay" },
//         { "Inventory", "UI Inventory" },
//         { "Setting", "UI Setting" },
//         { "Settings", "UI Setting" },
//         { "Menu", "UI Menu" },
//         { "Back", "UI Menu" },
//         { "Home", "UI Menu" },
//     };

//     private static readonly Dictionary<string, string> ButtonNameSceneMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
//     {
//         { "BUTTONSETTING", "UI Setting" },
//         { "IMAGESETTING", "UI Setting" },
//         { "SETTINGIMAGE", "UI Setting" },
//         { "BUTTONINVENTORY", "UI Inventory" },
//         { "IMAGEINVENTORY", "UI Inventory" },
//         { "INVENTORY", "UI Inventory" }, // <--- THÊM DÒNG NÀY
//     };

//     private static readonly HashSet<string> LanguageButtonNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
//     {
//         "BUTTONWEB",
//         "BUTTONLANGUAGE",
//         "BUTTONLANG",
//         "BUTTONLANGUAGEICON",
//         "BUTTONLANGICON",
//         "LANGUAGE",
//         "WORLDWIDEWEB",
//         "IMAGEWORLDWIDEWEB",
//     };

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     private static void BootstrapBeforeSceneLoad()
//     {
//         Bootstrap();
//     }

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//     private static void Bootstrap()
//     {
//         BoundInstanceIds.Clear();
//         ClickTargets.Clear();
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//         SceneManager.sceneLoaded += OnSceneLoaded;
//         EnsureRuntimeInput();
//         BindScene(SceneManager.GetActiveScene());
//     }

//     private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//     {
//         BindScene(scene);
//     }

//     private static void BindScene(Scene scene)
//     {
//         if (!scene.IsValid() || !scene.isLoaded)
//             return;

//         EnsureEventSystem();

//         BindTapTexts(scene);
//         BindButtons(scene);
//         BindGraphics(scene);
//         RegisterClickTargets(scene);
//     }

//     private static void BindTapTexts(Scene scene)
//     {
//         var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
//         foreach (var tmp in texts)
//         {
//             if (tmp == null)
//                 continue;
//             if (tmp.gameObject.scene != scene)
//                 continue;
//             if (!IsTapTextName(tmp.gameObject.name))
//                 continue;
//             if (!TryMarkBound(tmp))
//                 continue;

//             if (!tmp.raycastTarget)
//                 tmp.raycastTarget = true;

//             var button = tmp.GetComponent<TapTextButton>();
//             if (button == null)
//                 button = tmp.gameObject.AddComponent<TapTextButton>();

//             if (string.IsNullOrWhiteSpace(button.SceneName))
//             {
//                 var key = tmp.text != null ? tmp.text.Trim() : string.Empty;
//                 if (SceneMap.TryGetValue(key, out var targetScene))
//                     button.SetSceneName(targetScene);
//             }
//         }
//     }

//     private static void BindButtons(Scene scene)
//     {
//         var buttons = Resources.FindObjectsOfTypeAll<Button>();
//         foreach (var button in buttons)
//         {
//             if (button == null)
//                 continue;
//             if (button.gameObject.scene != scene)
//                 continue;
//             if (!TryMarkBound(button))
//                 continue;

//             if (TryBindButtonByName(button))
//                 continue;

//             var label = button.GetComponentInChildren<TMP_Text>(true);
//             if (label == null)
//                 continue;

//             var key = label.text != null ? label.text.Trim() : string.Empty;
//             if (SceneMap.TryGetValue(key, out var targetScene))
//             {
//                 var targetSceneCopy = targetScene;
//                 button.onClick.AddListener(() => LoadScene(targetSceneCopy));
//                 button.interactable = true;
//                 button.enabled = true;
//                 EnsureButtonRaycast(button);
//             }
//         }
//     }

//     private static bool TryBindButtonByName(Button button)
//     {
//         var normalized = NormalizeName(button.gameObject.name);
//         if (!TryGetActionForName(normalized, out var action))
//             return false;

//         button.onClick.RemoveAllListeners();
//         button.onClick.AddListener(() => action());
//         button.interactable = true;
//         button.enabled = true;
//         EnsureButtonRaycast(button);
//         BringToFront(button.transform);
//         return true;
//     }

//     private static void BindGraphics(Scene scene)
//     {
//         var graphics = Resources.FindObjectsOfTypeAll<Graphic>();
//         foreach (var graphic in graphics)
//         {
//             if (graphic == null)
//                 continue;
//             if (graphic.gameObject.scene != scene)
//                 continue;
//             if (graphic.GetComponent<Button>() != null)
//                 continue;
//             if (graphic.GetComponentInParent<TMP_Text>() != null)
//                 continue;
//             if (!TryMarkBound(graphic))
//                 continue;

//             if (!TryGetActionForTransform(graphic.transform, out var action))
//                 continue;

//             if (!graphic.raycastTarget)
//                 graphic.raycastTarget = true;

//             var tap = graphic.GetComponent<TapGraphicButton>();
//             if (tap == null)
//                 tap = graphic.gameObject.AddComponent<TapGraphicButton>();

//             tap.SetOnClick(action);
//             BringToFront(graphic.transform);
//         }
//     }

//     private static void RegisterClickTargets(Scene scene)
//     {
//         var graphics = Resources.FindObjectsOfTypeAll<Graphic>();
//         foreach (var graphic in graphics)
//         {
//             if (graphic == null)
//                 continue;
//             if (graphic.gameObject.scene != scene)
//                 continue;

//             if (TryGetActionForTransform(graphic.transform, out var action))
//                 ClickTargets.Add(new ClickTarget(graphic.rectTransform, action, ResolveCameraForGraphic(graphic)));
//         }
//     }

//     private static bool IsTapTextName(string name)
//     {
//         if (string.IsNullOrWhiteSpace(name))
//             return false;

//         return name.Trim().StartsWith("Tap_Text", StringComparison.OrdinalIgnoreCase);
//     }

//     private static string NormalizeName(string value)
//     {
//         if (string.IsNullOrWhiteSpace(value))
//             return string.Empty;

//         var chars = value.Trim();
//         var buffer = new char[chars.Length];
//         int count = 0;
//         foreach (var c in chars)
//         {
//             if (char.IsLetterOrDigit(c))
//                 buffer[count++] = char.ToUpperInvariant(c);
//         }

//         return new string(buffer, 0, count);
//     }

//     private static void EnsureButtonRaycast(Button button)
//     {
//         if (button.targetGraphic != null && !button.targetGraphic.raycastTarget)
//             button.targetGraphic.raycastTarget = true;
//     }

//     private static bool IsSpecialGraphicName(string normalized)
//     {
//         return LanguageButtonNames.Contains(normalized) || ButtonNameSceneMap.ContainsKey(normalized);
//     }

//     private static bool TryGetActionForName(string normalized, out Action action)
//     {
//         action = null;
//         if (string.IsNullOrWhiteSpace(normalized))
//             return false;

//         if (LanguageButtonNames.Contains(normalized))
//         {
//             action = ToggleLanguage;
//             return true;
//         }

//         if (ButtonNameSceneMap.TryGetValue(normalized, out var targetScene))
//         {
//             var targetSceneCopy = targetScene;
//             action = () => LoadScene(targetSceneCopy);
//             return true;
//         }

//         if (normalized.Contains("SETTING"))
//         {
//             action = () => LoadScene("UI Setting");
//             return true;
//         }

//         if (normalized.Contains("INVENTORY") || normalized.Contains("BACKPACK") || normalized.Contains("BAG"))
//         {
//             action = () => LoadScene("UI Inventory");
//             return true;
//         }

//         if (normalized.Contains("LANG") || normalized.Contains("WORLD") || normalized.Contains("WEB"))
//         {
//             action = ToggleLanguage;
//             return true;
//         }

//         return false;
//     }

//     private static bool TryGetActionForTransform(Transform target, out Action action)
//     {
//         action = null;
//         if (target == null)
//             return false;

//         var current = target;
//         int depth = 0;
//         while (current != null && depth < 8)
//         {
//             var normalized = NormalizeName(current.gameObject.name);
//             if (TryGetActionForName(normalized, out action))
//                 return true;

//             current = current.parent;
//             depth++;
//         }

//         return false;
//     }

//     private static bool TryMarkBound(Component component)
//     {
//         var id = component.GetInstanceID();
//         if (BoundInstanceIds.Contains(id))
//             return false;

//         BoundInstanceIds.Add(id);
//         return true;
//     }

//     private static void BringToFront(Transform target)
//     {
//         if (target == null)
//             return;

//         target.SetAsLastSibling();
//     }

//     private static void EnsureRuntimeInput()
//     {
//         if (s_runtimeInput != null)
//             return;

//         var existing = UnityEngine.Object.FindObjectOfType<MenuUIRuntimeInput>();
//         if (existing != null)
//         {
//             s_runtimeInput = existing;
//             return;
//         }

//         var go = new GameObject("MenuUIRuntime");
//         s_runtimeInput = go.AddComponent<MenuUIRuntimeInput>();
//         UnityEngine.Object.DontDestroyOnLoad(go);
//     }

//     private static void HandleGlobalClick(Vector2 screenPosition)
//     {
//         if (TryHandleRaycastClick(screenPosition))
//             return;

//         TryHandleClickTargets(screenPosition);
//     }

//     private static bool TryHandleRaycastClick(Vector2 screenPosition)
//     {
//         var eventSystem = EventSystem.current;
//         if (eventSystem == null)
//             return false;

//         RaycastResults.Clear();
//         var data = new PointerEventData(eventSystem)
//         {
//             position = screenPosition
//         };

//         eventSystem.RaycastAll(data, RaycastResults);
//         for (int i = 0; i < RaycastResults.Count; i++)
//         {
//             var result = RaycastResults[i];
//             var go = result.gameObject;
//             if (go == null)
//                 continue;

//             if (go.GetComponentInParent<Button>() != null)
//                 return true;

//             if (go.GetComponentInParent<TapGraphicButton>() != null)
//                 return true;

//             if (go.GetComponentInParent<TapTextButton>() != null)
//                 return true;

//             if (TryGetActionForTransform(go.transform, out var action))
//             {
//                 action?.Invoke();
//                 return true;
//             }
//         }

//         return false;
//     }

//     private static bool TryHandleClickTargets(Vector2 screenPosition)
//     {
//         for (int i = 0; i < ClickTargets.Count; i++)
//         {
//             var target = ClickTargets[i];
//             if (target.Rect == null || !target.Rect.gameObject.activeInHierarchy)
//                 continue;

//             if (RectTransformUtility.RectangleContainsScreenPoint(target.Rect, screenPosition, target.Camera))
//             {
//                 target.Action?.Invoke();
//                 return true;
//             }
//         }

//         return false;
//     }

//     private static Camera ResolveCameraForGraphic(Graphic graphic)
//     {
//         if (graphic == null)
//             return null;

//         var canvas = graphic.canvas != null ? graphic.canvas : graphic.GetComponentInParent<Canvas>();
//         if (canvas == null)
//             return null;

//         if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
//             return null;

//         if (canvas.worldCamera != null)
//             return canvas.worldCamera;

//         return Camera.main;
//     }

//     private sealed class ClickTarget
//     {
//         public RectTransform Rect;
//         public Action Action;
//         public Camera Camera;

//         public ClickTarget(RectTransform rect, Action action, Camera camera)
//         {
//             Rect = rect;
//             Action = action;
//             Camera = camera;
//         }
//     }

//     private sealed class MenuUIRuntimeInput : MonoBehaviour
//     {
//         private void Update()
//         {
//             if (Input.touchCount > 0)
//             {
//                 var touch = Input.GetTouch(0);
//                 if (touch.phase == TouchPhase.Began)
//                     HandleGlobalClick(touch.position);
//                 return;
//             }

//             if (Input.GetMouseButtonDown(0))
//                 HandleGlobalClick(Input.mousePosition);
//         }
//     }

//     private static void ToggleLanguage()
//     {
//         var current = PlayerPrefs.GetString(LanguagePrefKey, "en");
//         var next = string.Equals(current, "en", StringComparison.OrdinalIgnoreCase) ? "vi" : "en";
//         PlayerPrefs.SetString(LanguagePrefKey, next);
//         PlayerPrefs.Save();
//     }

//     private static void LoadScene(string sceneName)
//     {
//         if (string.IsNullOrWhiteSpace(sceneName))
//             return;

//         if (Application.CanStreamedLevelBeLoaded(sceneName))
//         {
//             SceneManager.LoadScene(sceneName);
//             return;
//         }

// #if UNITY_EDITOR
//         var scenePath = FindScenePath(sceneName);
//         if (!string.IsNullOrWhiteSpace(scenePath))
//         {
//             EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
//             return;
//         }
// #endif
//     }

// #if UNITY_EDITOR
//     private static string FindScenePath(string sceneName)
//     {
//         var guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
//         foreach (var guid in guids)
//         {
//             var path = AssetDatabase.GUIDToAssetPath(guid);
//             if (path.EndsWith($"/{sceneName}.unity", StringComparison.OrdinalIgnoreCase))
//                 return path;
//         }

//         return null;
//     }
// #endif

//     private static void EnsureEventSystem()
//     {
// #if UNITY_2023_1_OR_NEWER
//         if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
//             return;
// #else
//         if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
//             return;
// #endif

//         var eventSystemGo = new GameObject("EventSystem");
//         eventSystemGo.AddComponent<EventSystem>();
// #if ENABLE_INPUT_SYSTEM
//         eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
// #else
//         eventSystemGo.AddComponent<StandaloneInputModule>();
// #endif
//     }
// }
