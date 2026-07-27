using UnityEngine;

public class InteractGoldReward : MonoBehaviour
{
    [Header("Gold Reward")]
    [SerializeField] private int goldAmount = 100;

    [Header("Destroy After Collect")]
    [SerializeField] private bool destroyAfterCollect = true;

    public void OnInteract()
    {
        if (GoldManager.Instance == null)
            return;

        GoldManager.Instance.AddGold(goldAmount);

        Debug.Log($"Đã nhận {goldAmount} vàng");

        if (destroyAfterCollect)
        {
            Destroy(gameObject);
        }
    }
}