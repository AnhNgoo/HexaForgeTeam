using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayInteractionZone : MonoBehaviour
{
    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionUI;

    [Header("Game Scene")]
    [SerializeField] private string gameSceneName = "Game Scene";

    [Header("Interaction Key")]
    [SerializeField] private KeyCode playKey = KeyCode.F;

    private bool playerInside;
    private bool isLoading;

    private void Start()
    {
        SetInteractionUI(false);
    }

    private void Update()
    {
        if (!playerInside || isLoading)
            return;

        if (Input.GetKeyDown(playKey))
        {
            StartGame();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = true;
        SetInteractionUI(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = false;
        SetInteractionUI(false);
    }

    private void StartGame()
    {
        if (isLoading)
            return;

        isLoading = true;
        SetInteractionUI(false);

        Time.timeScale = 1f;

        Debug.Log("Loading game scene: " + gameSceneName);

        // Single: xóa Lobby và chuyển hẳn sang gameplay.
        SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void SetInteractionUI(bool state)
    {
        if (interactionUI != null &&
            interactionUI.activeSelf != state)
        {
            interactionUI.SetActive(state);
        }
    }

    private void OnDisable()
    {
        SetInteractionUI(false);
    }
}