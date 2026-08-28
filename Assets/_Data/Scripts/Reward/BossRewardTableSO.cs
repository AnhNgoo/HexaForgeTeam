using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossRewardCandidate
{
    public BossRewardDataSO reward;

    [Min(1)]
    [Tooltip("Trọng số tương đối, không phải phần trăm bắt buộc.")]
    public int weight = 1;
}

[Serializable]
public class BossRewardSlot
{
    public List<BossRewardCandidate> candidates = new();
}

[CreateAssetMenu(fileName = "BossRewardTable", menuName = "Enemy/Boss Reward/Reward Table")]
public class BossRewardTableSO : ScriptableObject
{
    [Tooltip("Mỗi phần tử tạo một card. Chỉ dùng từ 2 đến 3 slot.")]
    [SerializeField] private List<BossRewardSlot> rewardSlots = new();

    public List<BossRewardDataSO> RollRewards()
    {
        List<BossRewardDataSO> results = new();
        HashSet<string> usedKeys = new();
        int slotCount = Mathf.Min(3, rewardSlots.Count);

        for (int i = 0; i < slotCount; i++)
        {
            BossRewardDataSO reward = RollSlot(rewardSlots[i], usedKeys);

            if (reward == null)
            {
                Debug.LogWarning(
                    $"{name}: Reward Slot {i} không còn candidate hợp lệ.");
                continue;
            }

            results.Add(reward);
            usedKeys.Add(reward.UniqueKey);
        }

        return results;
    }

    private BossRewardDataSO RollSlot(BossRewardSlot slot, HashSet<string> usedKeys)
    {
        if (slot?.candidates == null)
            return null;

        int totalWeight = 0;

        foreach (BossRewardCandidate candidate in slot.candidates)
        {
            if (IsValid(candidate, usedKeys))
                totalWeight += Mathf.Max(1, candidate.weight);
        }

        if (totalWeight <= 0)
            return null;

        int roll = UnityEngine.Random.Range(0, totalWeight);

        foreach (BossRewardCandidate candidate in slot.candidates)
        {
            if (!IsValid(candidate, usedKeys))
                continue;

            roll -= Mathf.Max(1, candidate.weight);
            if (roll < 0) return candidate.reward;
        }

        return null;
    }

    private static bool IsValid(BossRewardCandidate candidate, HashSet<string> usedKeys)
    {
        return candidate?.reward != null && candidate.reward.IsConfigured && !usedKeys.Contains(candidate.reward.UniqueKey);
    }
}
