using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LobbyUIOverlayManager : MonoBehaviour
{
    public static LobbyUIOverlayManager Instance { get; private set; }

    [Header("UI Scene")]
    [SerializeField] private string uiSceneName = "UI Menu";

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Behaviour[] playerControls;

    [Header("Event System")]
    [SerializeField] private EventSystem lobbyEventSystem;

    [Header("Settings")]
    [SerializeField] private bool pauseWorldWhileUIOpen;
    [SerializeField] private bool unlockCursorWhileUIOpen = true;

    public bool IsUIOpen { get; private set; }
    public bool IsBusy { get; private set; }

    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;

    private bool[] previousControlStates;

    private CharacterController playerController;
    private bool controllerWasEnabled;

    private Rigidbody playerRigidbody;

    private float previousTimeScale;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (lobbyEventSystem == null)
            lobbyEventSystem = FindObjectOfType<EventSystem>();

        if (player != null)
        {
            playerController =
                player.GetComponent<CharacterController>();

            playerRigidbody =
                player.GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        if (!IsUIOpen || IsBusy)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            CloseMenu();
    }

    public void OpenMenu(MenuType targetMenu)
    {
        if (IsUIOpen || IsBusy)
            return;

        StartCoroutine(OpenMenuRoutine(targetMenu));
    }

    public void CloseMenu()
    {
        if (!IsUIOpen || IsBusy)
            return;

        StartCoroutine(CloseMenuRoutine());
    }

    private IEnumerator OpenMenuRoutine(MenuType targetMenu)
    {
        IsBusy = true;
        IsUIOpen = true;

        SaveLobbyState();
        DisablePlayerControls();
        ConfigureCursorForUI();

        // Lobby có EventSystem riêng, tắt nó để không bị trùng
        // với EventSystem trong scene UI Menu.
        if (lobbyEventSystem != null)
            lobbyEventSystem.gameObject.SetActive(false);

        if (pauseWorldWhileUIOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        Scene uiScene = SceneManager.GetSceneByName(uiSceneName);

        if (!uiScene.isLoaded)
        {
            AsyncOperation loadOperation =
                SceneManager.LoadSceneAsync(
                    uiSceneName,
                    LoadSceneMode.Additive
                );

            if (loadOperation == null)
            {
                Debug.LogError(
                    "Không thể load scene: " + uiSceneName
                );

                RestoreLobbyAfterClose();

                IsUIOpen = false;
                IsBusy = false;

                yield break;
            }

            while (!loadOperation.isDone)
                yield return null;
        }

        // Chờ UIManager trong scene UI Menu khởi tạo.
        float timeout = 5f;

        while (UIManager.Instance == null && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (UIManager.Instance == null)
        {
            Debug.LogError(
                "Scene UI Menu không có UIManager hoặc UIManager chưa khởi tạo."
            );

            Scene loadedUIScene =
                SceneManager.GetSceneByName(uiSceneName);

            if (loadedUIScene.isLoaded)
            {
                AsyncOperation unloadOperation =
                    SceneManager.UnloadSceneAsync(loadedUIScene);

                if (unloadOperation != null)
                {
                    while (!unloadOperation.isDone)
                        yield return null;
                }
            }

            RestoreLobbyAfterClose();

            IsUIOpen = false;
            IsBusy = false;

            yield break;
        }

        UIManager.Instance.ChangeMenu(targetMenu);

        IsBusy = false;

        Debug.Log("Opened UI: " + targetMenu);
    }

    private IEnumerator CloseMenuRoutine()
    {
        IsBusy = true;

        // Để bảo đảm coroutine và gameplay trở về trạng thái bình thường.
        Time.timeScale = 1f;

        Scene uiScene =
            SceneManager.GetSceneByName(uiSceneName);

        if (uiScene.isLoaded)
        {
            AsyncOperation unloadOperation =
                SceneManager.UnloadSceneAsync(uiScene);

            if (unloadOperation != null)
            {
                while (!unloadOperation.isDone)
                    yield return null;
            }
        }

        RestoreLobbyAfterClose();

        IsUIOpen = false;
        IsBusy = false;

        Debug.Log("Closed UI and returned to Lobby");
    }

    private void SaveLobbyState()
    {
        if (player != null)
        {
            savedPlayerPosition = player.position;
            savedPlayerRotation = player.rotation;
        }

        if (playerController != null)
            controllerWasEnabled = playerController.enabled;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        if (!pauseWorldWhileUIOpen)
            previousTimeScale = Time.timeScale;
    }

    private void DisablePlayerControls()
    {
        if (playerControls == null)
            return;

        previousControlStates =
            new bool[playerControls.Length];

        for (int i = 0; i < playerControls.Length; i++)
        {
            Behaviour control = playerControls[i];

            if (control == null)
                continue;

            previousControlStates[i] = control.enabled;
            control.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void RestorePlayerControls()
    {
        if (playerControls == null ||
            previousControlStates == null)
        {
            return;
        }

        for (int i = 0; i < playerControls.Length; i++)
        {
            Behaviour control = playerControls[i];

            if (control == null)
                continue;

            control.enabled = previousControlStates[i];
        }
    }

    private void RestorePlayerTransform()
    {
        if (player == null)
            return;

        if (playerController != null)
            playerController.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.position = savedPlayerPosition;
            playerRigidbody.rotation = savedPlayerRotation;
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            player.SetPositionAndRotation(
                savedPlayerPosition,
                savedPlayerRotation
            );
        }

        if (playerController != null)
            playerController.enabled = controllerWasEnabled;
    }

    private void ConfigureCursorForUI()
    {
        if (!unlockCursorWhileUIOpen)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
    }

    private void RestoreLobbyAfterClose()
    {
        RestorePlayerTransform();
        RestorePlayerControls();
        RestoreCursor();

        Time.timeScale = previousTimeScale;

        if (lobbyEventSystem != null)
            lobbyEventSystem.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}