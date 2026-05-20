using UnityEngine;

public class GoldManager : Singleton<GoldManager>
{
    [Header("Gold Runtime")]
    [SerializeField] private int currentGold = 0;
    public int CurrentGold => currentGold;

    /// <summary>
    /// Cộng vàng
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold += amount;

        Debug.Log($"Nhận {amount} vàng | Tổng vàng: {currentGold}");
    }

    /// <summary>
    /// Trừ vàng
    /// </summary>
    public void RemoveGold(int amount)
    {
        if (amount <= 0)
            return;

        currentGold -= amount;

        if (currentGold < 0)
            currentGold = 0;

        Debug.Log($"Mất {amount} vàng | Tổng vàng: {currentGold}");
    }

    /// <summary>
    /// Kiểm tra đủ vàng không
    /// </summary>
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    /// <summary>
    /// Reset vàng
    /// </summary>
    public void ResetGold()
    {
        currentGold = 0;
    }
}