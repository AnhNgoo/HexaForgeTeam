using UnityEngine;

public class LevelUpMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private TestPlayerMovement playerMovement;

    private void Awake()
    {
        if (panel == null)
            panel = gameObject;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerMovement = player.GetComponent<TestPlayerMovement>();
        }
    }

    public void Open()
    {
        panel.SetActive(true);

        if (playerMovement != null)
        {
            playerMovement.IsLockMovement = true;
        }
    }

    public void Close()
    {
        panel.SetActive(false);

        if (playerMovement != null)
        {
            playerMovement.IsLockMovement = false;
        }
    }

    public void OnClickLevelUp()
    {
        LevelManager.Instance?.LevelUp();
    }
}