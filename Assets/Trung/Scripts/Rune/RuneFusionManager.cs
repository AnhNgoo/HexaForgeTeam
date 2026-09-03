using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuneFusionManager : MonoBehaviour
{
    public static RuneFusionManager Instance;

    [Header("Fusion Success Rates (GDD Standard %)")]
    [Range(0f, 100f)] [SerializeField] private float commonToRareRate = 85f;
    [Range(0f, 100f)] [SerializeField] private float rareToEpicRate = 60f;
    [Range(0f, 100f)] [SerializeField] private float epicToLegendaryRate = 35f;

    [Header("Rune Shard Cost (GDD Standard)")]
    [SerializeField] private int costCommon = 100;
    [SerializeField] private int costRare = 300;
    [SerializeField] private int costEpic = 800;

    [Header("Protection Item Config")]
    [SerializeField] private string charmItemID = "FUSION_CHARM_01";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool TryFuseRunes(List<string> ingredientRuneIDs, bool useProtection, out bool isSuccess, out RuneData resultRune)
    {
        isSuccess = false;
        resultRune = null;

        if (ingredientRuneIDs == null || ingredientRuneIDs.Count != 3)
        {
            Debug.LogError("[ÉP NGỌC] Đầu vào phải có đúng 3 nguyên liệu!");
            return false;
        }

        List<RuneData> ingredients = new List<RuneData>();
        foreach (string id in ingredientRuneIDs)
        {
            RuneData r = RuneInventoryManager.Instance.runes.FirstOrDefault(x => x.runeID == id);
            if (r != null) ingredients.Add(r);
        }

        if (ingredients.Count != 3)
        {
            Debug.LogError("[ÉP NGỌC] Không tìm thấy đủ 3 viên ngọc nguyên liệu trong hòm đồ!");
            return false;
        }

        RuneRarity materialRarity = ingredients[0].runeRarity;
        if (materialRarity == RuneRarity.Legendary)
        {
            Debug.LogError("[ÉP NGỌC] Cấp bậc Huyền thoại (Legendary) đã là tối đa, không thể dung hợp thêm!");
            return false;
        }

        foreach (RuneData r in ingredients)
        {
            if (r.runeRarity != materialRarity)
            {
                Debug.LogError("[ÉP NGỌC] Cả 3 nguyên liệu mồi phải cùng một cấp độ hiếm!");
                return false;
            }
        }

        int shardCost = GetFusionCost(materialRarity);
        if (RuneShardManager.Instance == null || RuneShardManager.Instance.GetCurrentShards() < shardCost)
        {
            Debug.LogWarning("[ÉP NGỌC] Không đủ Rune Shards để tiến hành dung hợp!");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Not enough Rune Shards!", Color.red);
            return false;
        }

        bool isCharmApplied = useProtection && InventoryItemManager.Instance != null && InventoryItemManager.Instance.GetItemQuantity(charmItemID) >= 1;

        if (isCharmApplied)
        {
            InventoryItemManager.Instance.SpendItem(charmItemID, 1);
            RuneShardManager.Instance.SpendShards(shardCost);
        }
        else
        {
            RuneShardManager.Instance.SpendShards(shardCost);
        }

        // Tự động tháo ngọc khỏi ô Equip nếu nguyên liệu đang được đeo
        if (CharacterManager.Instance != null)
        {
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                if (build != null && build.equippedRuneIDs != null)
                {
                    for (int slot = 0; slot < build.equippedRuneIDs.Length; slot++)
                    {
                        if (ingredientRuneIDs.Contains(build.equippedRuneIDs[slot]))
                        {
                            build.equippedRuneIDs[slot] = "";
                        }
                    }
                }
            }
        }

        if (RuneInventoryManager.Instance != null)
        {
            RuneInventoryManager.Instance.RemoveRunesRange(ingredientRuneIDs);
        }

        if (RuneEquipUI.Instance != null)
        {
            RuneEquipUI.Instance.RefreshEquipUI();
        }

        float roll = Random.Range(0f, 100f);
        float chance = isCharmApplied ? 100f : GetSuccessRate(materialRarity);

        if (roll <= chance)
        {
            isSuccess = true;
            RuneRarity nextRarity = materialRarity + 1;
            resultRune = GenerateRandomRune(nextRarity);

            if (RuneInventoryManager.Instance != null)
            {
                RuneInventoryManager.Instance.AddRune(resultRune);
            }

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.AddFusionProgress(1);
            }

            Debug.Log($"[ÉP NGỌC] Đập đồ thành công! Nhận được: {resultRune.runeName} ({resultRune.runeRarity})");
        }
        else
        {
            isSuccess = false;
            int refundShards = Mathf.RoundToInt(shardCost * 0.2f);
            if (RuneShardManager.Instance != null && refundShards > 0)
            {
                RuneShardManager.Instance.AddShards(refundShards);
            }
            Debug.Log($"[ÉP NGỌC] Đập đồ thất bại. Hoàn trả 20% chi phí (+{refundShards} Shards).");
        }

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.RefreshInventory();
        }

        return true;
    }

    private int GetFusionCost(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return costCommon;
            case RuneRarity.Rare: return costRare;
            case RuneRarity.Epic: return costEpic;
        }
        return 0;
    }

    private float GetSuccessRate(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return commonToRareRate;
            case RuneRarity.Rare: return rareToEpicRate;
            case RuneRarity.Epic: return epicToLegendaryRate;
        }
        return 0f;
    }

    private RuneData GenerateRandomRune(RuneRarity rarity)
    {
        RuneColor randomColor = (RuneColor)Random.Range(0, 3);
        RuneData newRune = new RuneData(randomColor, rarity);

        AssignRuneLore(newRune);
        GenerateAffixes(newRune);

        return newRune;
    }

    private void GenerateAffixes(RuneData rune)
    {
        int affixCount = GetAffixCount(rune.runeRarity);
        List<RuneStatType> usedStats = new List<RuneStatType>();

        for (int i = 0; i < affixCount; i++)
        {
            RuneStatType statType = GetRandomStat(usedStats);
            usedStats.Add(statType);
            float value = GetRandomValue(statType, rune.runeRarity);
            rune.affixes.Add(new RuneAffixData(statType, value));
        }
    }

    private int GetAffixCount(RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common: return 1;
            case RuneRarity.Rare: return 2;
            case RuneRarity.Epic: return 3;
            case RuneRarity.Legendary: return 4;
        }
        return 1;
    }

    private RuneStatType GetRandomStat(List<RuneStatType> usedStats)
    {
        List<RuneStatType> pool = new List<RuneStatType>()
        {
            RuneStatType.HP, RuneStatType.HPPercent,
            RuneStatType.MP, RuneStatType.MPPercent, RuneStatType.MPRegen,
            RuneStatType.Stamina, RuneStatType.StaminaPercent, RuneStatType.StaminaRegen,
            RuneStatType.ATK, RuneStatType.ATKPercent,
            RuneStatType.DEF, RuneStatType.DEFPercent,
            RuneStatType.Speed, RuneStatType.PoisonDamage
        };

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (usedStats.Contains(pool[i])) pool.RemoveAt(i);
        }
        return pool[Random.Range(0, pool.Count)];
    }

    private float GetRandomValue(RuneStatType statType, RuneRarity rarity)
    {
        switch (statType)
        {
            case RuneStatType.HP: return GetValueByRarity(rarity, 80f, 180f, 180f, 350f, 350f, 650f, 650f, 1200f);
            case RuneStatType.MP: return GetValueByRarity(rarity, 25f, 60f, 60f, 120f, 120f, 220f, 220f, 400f);
            case RuneStatType.Stamina: return GetValueByRarity(rarity, 15f, 40f, 40f, 80f, 80f, 140f, 140f, 250f);
            case RuneStatType.ATK: return GetValueByRarity(rarity, 3f, 8f, 8f, 18f, 18f, 35f, 35f, 60f);
            case RuneStatType.DEF: return GetValueByRarity(rarity, 2f, 6f, 6f, 14f, 14f, 28f, 28f, 50f);
            case RuneStatType.HPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 20f);
            case RuneStatType.MPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.StaminaPercent: return GetValueByRarity(rarity, 3f, 5f, 5f, 9f, 9f, 15f, 15f, 25f);
            case RuneStatType.ATKPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.DEFPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.StaminaRegen: return GetValueByRarity(rarity, 3f, 6f, 6f, 10f, 10f, 18f, 18f, 30f);
            case RuneStatType.MPRegen: return GetValueByRarity(rarity, 1f, 3f, 3f, 6f, 6f, 10f, 10f, 16f);
            case RuneStatType.Speed: return GetValueByRarity(rarity, 0.2f, 0.5f, 0.5f, 0.9f, 0.9f, 1.4f, 1.4f, 2.2f);
            case RuneStatType.PoisonDamage: return GetValueByRarity(rarity, 2f, 5f, 5f, 10f, 10f, 20f, 20f, 35f);
        }
        return 1f;
    }

    private float GetValueByRarity(RuneRarity rarity, float cMin, float cMax, float rMin, float rMax, float eMin, float eMax, float lMin, float lMax)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return Random.Range(cMin, cMax);
            case RuneRarity.Rare: return Random.Range(rMin, rMax);
            case RuneRarity.Epic: return Random.Range(eMin, eMax);
            case RuneRarity.Legendary: return Random.Range(lMin, lMax);
        }
        return 1f;
    }

    private void AssignRuneLore(RuneData rune)
    {
        switch (rune.runeColor)
        {
            case RuneColor.Red: AssignRedLore(rune); break;
            case RuneColor.Green: AssignGreenLore(rune); break;
            case RuneColor.Blue: AssignBlueLore(rune); break;
        }
    }

    private void AssignRedLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Ashfang"; rune.runeLore = "Its heat faded long ago, yet the scar remains."; break;
            case RuneRarity.Rare: rune.runeName = "Blood Oath"; rune.runeLore = "The knight survived the battle. His comrades did not."; break;
            case RuneRarity.Epic: rune.runeName = "Heart of Ruin"; rune.runeLore = "Every beat echoed like a war drum beneath the earth."; break;
            case RuneRarity.Legendary: rune.runeName = "Crimson Crown"; rune.runeLore = "Kings burned kingdoms to wear it for a single night."; break;
        }
    }

    private void AssignGreenLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Wiltroot"; rune.runeLore = "It grew where no light should ever reach."; break;
            case RuneRarity.Rare: rune.runeName = "Verdant Pulse"; rune.runeLore = "The forest whispered back when spoken to."; break;
            case RuneRarity.Epic: rune.runeName = "Hollow Bloom"; rune.runeLore = "Flowers fed on the dead beneath the ruins."; break;
            case RuneRarity.Legendary: rune.runeName = "Worldsap Core"; rune.runeLore = "Its roots once held an entire civilization together."; break;
        }
    }

    private void AssignBlueLore(RuneData rune)
    {
        switch (rune.runeRarity)
        {
            case RuneRarity.Common: rune.runeName = "Frost Vein"; rune.runeLore = "Cold enough to silence fear itself."; break;
            case RuneRarity.Rare: rune.runeName = "Moon Shard"; rune.runeLore = "Fragments of a sky long forgotten."; break;
            case RuneRarity.Epic: rune.runeName = "Deep Current"; rune.runeLore = "Something ancient moved beneath the tide."; break;
            case RuneRarity.Legendary: rune.runeName = "Eye of Eternity"; rune.runeLore = "It watched the end before time understood death."; break;
        }
    }
}