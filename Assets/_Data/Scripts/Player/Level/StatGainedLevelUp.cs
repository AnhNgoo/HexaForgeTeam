using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatGainedLevelUp", menuName = "ScriptableObjects/StatGainedLevelUp", order = 1)]
public class StatGainedLevelUp : ScriptableObject
{
    public int maxLevel = 15;
    public List<LevelUpCost> levelUpCosts = new List<LevelUpCost>();
    public CharacterStats characterStats;

    public int GetLevelUpCost(int level)
    {
        LevelUpCost cost = levelUpCosts.Find(c => c.level == level);
        if (cost != null)
        {
            return cost.cost;
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy chi phí tăng cấp cho cấp độ {level}");
            return 0; // Hoặc giá trị mặc định khác nếu cần
        }
    }
}

[System.Serializable]
public class LevelUpCost
{
    public int level;
    public int cost;
}