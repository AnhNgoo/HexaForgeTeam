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

        // Lưu các candidate weapon sai hệ để những lần roll sau không chọn lại chúng.
        HashSet<BossRewardCandidate> rejectedCandidates = new();

        while (true)
        {
            BossRewardCandidate candidate = RollCandidate(slot.candidates, usedKeys, rejectedCandidates);
            if (candidate == null)
                return null;

            // Reward không phải weapon hoặc weapon đúng hệ thì nhận ngay.
            if (IsCompatibleWeapon(candidate.reward))
                return candidate.reward;

            // Chỉ weapon sai hệ mới bị loại và phải roll lại.
            rejectedCandidates.Add(candidate);
        }
    }

    private static BossRewardCandidate RollCandidate(
        List<BossRewardCandidate> candidates,
        HashSet<string> usedKeys,
        HashSet<BossRewardCandidate> rejectedCandidates)
    {
        int totalWeight = 0;

        // Tính tổng trọng số của các candidate hợp lệ và chưa bị loại tạm thời.
        foreach (BossRewardCandidate candidate in candidates)
        {
            if (IsValid(candidate, usedKeys) && !rejectedCandidates.Contains(candidate))
                totalWeight += Mathf.Max(1, candidate.weight);
        }

        if (totalWeight <= 0)
            return null;

        int roll = UnityEngine.Random.Range(0, totalWeight);

        // Dùng số roll để chọn candidate theo tỷ lệ weight.
        foreach (BossRewardCandidate candidate in candidates)
        {
            if (!IsValid(candidate, usedKeys) || rejectedCandidates.Contains(candidate))
                continue;

            roll -= Mathf.Max(1, candidate.weight);
            if (roll < 0)
                return candidate;
        }

        return null;
    }

    private static bool IsCompatibleWeapon(BossRewardDataSO reward)
    {
        // Reward chỉ tăng chỉ số thì không liên quan đến loại nhân vật.
        if (reward?.RewardType != BossRewardType.Weapon)
            return true;

        CharacterTypes characterType =
            PlayerManager.Instance?.CurrentCharacterBase?.CharacterData?.characterTypes ?? CharacterTypes.None;

        // Nhân vật cận chiến không được nhận MagicWand.
        if (characterType == CharacterTypes.PhysicalMelee)
            return reward.Weapon.weaponType != WeaponType.MagicWand;

        // Nhân vật phép thuật không được nhận vũ khí Melee.
        if (characterType == CharacterTypes.Magical)
            return reward.Weapon.weaponType != WeaponType.Melee;

        // Không xác định được hệ nhân vật thì không chặn reward.
        return true;
    }

    private static bool IsValid(BossRewardCandidate candidate, HashSet<string> usedKeys)
    {
        return candidate?.reward != null && candidate.reward.IsConfigured && !usedKeys.Contains(candidate.reward.UniqueKey);
    }
}
