using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Level")]
    [SerializeField] private int currentLevel = 1;
    public int CurrentLevel => currentLevel;

    [SerializeField] private int maxLevel = 15;

    [Header("Gold Cost")]
    [SerializeField] private List<int> goldCosts = new List<int>()
    {
        150,   // Lv1 -> Lv2
        250,   // Lv2 -> Lv3
        350,   // Lv3 -> Lv4
        450,   // Lv4 -> Lv5
        550,   // Lv5 -> Lv6
        650,   // Lv6 -> Lv7
        750,   // Lv7 -> Lv8
        850,   // Lv8 -> Lv9
        950,   // Lv9 -> Lv10
        1050,  // Lv10 -> Lv11
        1150,  // Lv11 -> Lv12
        1250,  // Lv12 -> Lv13
        1350,  // Lv13 -> Lv14
        1450   // Lv14 -> Lv15
    };

    /// <summary>
    /// Lấy giá vàng cần để lên cấp tiếp theo
    /// </summary>
    public int GetCurrentLevelUpCost()
    {
        if (currentLevel >= maxLevel)
            return 0;

        return goldCosts[currentLevel - 1];
    }

    /// <summary>
    /// Có đủ điều kiện lên cấp không
    /// </summary>
    public bool CanLevelUp()
    {
        if (currentLevel >= maxLevel)
            return false;

        return GoldManager.Instance.CurrentGold >= GetCurrentLevelUpCost();
    }

    /// <summary>
    /// Lên cấp
    /// </summary>
    public void LevelUp()
    {
        if (!CanLevelUp())
        {
            Debug.Log("Không đủ vàng hoặc đã max level");
            return;
        }

        int cost = GetCurrentLevelUpCost();

        GoldManager.Instance.RemoveGold(cost);

        currentLevel++;

        Debug.Log($"Đã lên Level {currentLevel}");
    }
}