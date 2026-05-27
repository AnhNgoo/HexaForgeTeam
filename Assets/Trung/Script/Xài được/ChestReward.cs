using System.Collections;
using UnityEngine;

public class ChestReward : MonoBehaviour
{
    [Header("Gold Reward")]
    [SerializeField] private int goldAmount = 100;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string openAnimation = "Open";
    [SerializeField] private string pressAnimation = "Press";

    [Header("Timing")]
    [SerializeField] private float openToPressDelay = 1.5f;

    [SerializeField] private float destroyDelay = 1f;

    private bool isOpened = false;

    private Collider chestCollider;

    private void Awake()
    {
        chestCollider = GetComponent<Collider>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        if (animator != null)
        {
            animator.Play(idleAnimation);
        }
    }

    public void OnInteract()
    {
        if (isOpened)
            return;

        isOpened = true;

        if (chestCollider != null)
        {
            chestCollider.enabled = false;
        }

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(goldAmount);

            Debug.Log($"Đã nhận {goldAmount} vàng");
        }

        StartCoroutine(OpenChestRoutine());
    }

    private IEnumerator OpenChestRoutine()
    {
        if (animator != null)
        {
            animator.Play(openAnimation);
        }

        yield return new WaitForSeconds(openToPressDelay);

        if (animator != null)
        {
            animator.Play(pressAnimation);
        }

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }
}