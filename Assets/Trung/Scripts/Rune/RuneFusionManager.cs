using System.Collections.Generic;
using UnityEngine;

public class RuneFusionManager : MonoBehaviour
{
    public static RuneFusionManager Instance;

    [Header("Fusion Success Rates (Tỷ lệ đập thành công %)")]
    [Range(0f, 100f)] [SerializeField] private float commonToRareRate = 85f;
    [Range(0f, 100f)] [SerializeField] private float rareToEpicRate = 60f;
    [Range(0f, 100f)] [SerializeField] private float epicToLegendaryRate = 35f;

    [Header("Gem Cost (Chi phí đập ngọc)")]
    [SerializeField] private int costCommon = 100;
    [SerializeField] private int costRare = 300;
    [SerializeField] private int costEpic = 800;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Hàm cốt lõi thực hiện việc ép ngọc từ danh sách 3 ID ngọc mồi
    /// </summary>
    public bool TryFuseRunes(List<string> ingredientRuneIDs, out bool isSuccess, out RuneData resultRune)
    {
        isSuccess = false;
        resultRune = null;

        // 1. Kiểm tra số lượng đầu vào
        if (ingredientRuneIDs == null || ingredientRuneIDs.Count != 3)
        {
            Debug.LogError("[Fusion] Phải bỏ đủ đúng 3 viên ngọc nguyên liệu!");
            return false;
        }

        if (RuneInventoryManager.Instance == null) return false;

        // 2. Trích xuất dữ liệu thực tế từ túi đồ để kiểm tra tính hợp lệ
        List<RuneData> ingredients = new List<RuneData>();
        foreach (string id in ingredientRuneIDs)
        {
            RuneData found = RuneInventoryManager.Instance.runes.Find(r => r.runeID == id);
            if (found != null) ingredients.Add(found);
        }

        if (ingredients.Count != 3)
        {
            Debug.LogError("[Fusion] Có viên ngọc nguyên liệu không tồn tại hoặc dữ liệu ma!");
            return false;
        }

        // 3. Kiểm tra xem 3 viên có cùng độ hiếm (Rarity) hay không
        RuneRarity baseRarity = ingredients[0].runeRarity;
        if (ingredients[1].runeRarity != baseRarity || ingredients[2].runeRarity != baseRarity)
        {
            Debug.LogWarning("[Fusion] 3 viên ngọc bắt buộc phải cùng độ hiếm!");
            return false;
        }

        if (baseRarity == RuneRarity.Legendary)
        {
            Debug.LogWarning("[Fusion] Ngọc Huyền Thoại đã là cấp tối đa, không thể nâng cấp thêm!");
            return false;
        }

        // =========================================================================
        // SỬA ĐỔI TIỀN TỆ: KIỂM TRA SỐ DƯ RUNE SHARDS VÀ KHẤU TRỪ TIỀN AN TOÀN
        // =========================================================================
        int requiredShards = GetFusionCost(baseRarity);

        if (RuneShardManager.Instance == null || RuneShardManager.Instance.GetCurrentShards() < requiredShards)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Not enough Rune Shards for Fusion!", Color.red);
            }
            return false; // Chặn đập đồ, giữ nguyên vẹn 3 viên ngọc mồi
        }

        // Thực hiện trừ tiền thông qua hàm SpendShards an toàn mới tạo
        RuneShardManager.Instance.SpendShards(requiredShards);

        // 5. Tính toán tỷ lệ Đỏ/Đen (Thành công hay Hụt)
        float successChance = GetSuccessRate(baseRarity);
        float roll = Random.Range(0f, 100f);

        // 6. Tiến hành xóa sạch 3 viên ngọc nguyên liệu khỏi túi đồ
        foreach (RuneData r in ingredients)
        {
            RuneInventoryManager.Instance.RemoveRune(r.runeID);
        }

        if (roll <= successChance)
        {
            // === ĐẬP TRÚNG (THÀNH CÔNG) ===
            isSuccess = true;
            
            // Ép trực tiếp bậc hiếm tiếp theo
            RuneRarity nextRarity = baseRarity + 1; 
            
            // Sinh ngọc ngẫu nhiên mới dựa trên độ hiếm thế hệ tiếp theo
            resultRune = GenerateRandomRune(nextRarity);
            RuneInventoryManager.Instance.AddRune(resultRune);
            Debug.Log($"[Fusion] ĐẬP ĐỒ THÀNH CÔNG! Bạn nhận được ngọc {nextRarity}: {resultRune.runeName}");
        }
        else
        {
            // === ĐẬP HỤT (THẤT BẠI) ===
            isSuccess = false;
            Debug.Log("[Fusion] ĐẬP ĐỒ THẤT BẠI! Ngọc nguyên liệu đã tan thành mây khói...");
        }

        // Làm mới lại hòm đồ UI sau khi đổi dữ liệu túi đồ
        if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();
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
            case RuneRarity.Common: return commonToRareRate; // 85%
            case RuneRarity.Rare: return rareToEpicRate;     // 60%
            case RuneRarity.Epic: return epicToLegendaryRate; // 35%
        }
        return 0f;
    }

    /// <summary>
    /// Hàm sinh chỉ số ngọc ngẫu nhiên cho viên ngọc mới ra lò
    /// </summary>
    private RuneData GenerateRandomRune(RuneRarity rarity)
    {
        RuneColor randomColor = (RuneColor)Random.Range(0, 3);
        RuneData newRune = new RuneData(randomColor, rarity);

        newRune.runeName = $"{rarity} {randomColor} Rune";
        newRune.runeLore = "A crystal forged from the ashes of fractured elements.";

        RuneStatType randomStat = (RuneStatType)Random.Range(0, 14);
        float randomValue = rarity == RuneRarity.Rare ? Random.Range(10f, 30f) : rarity == RuneRarity.Epic ? Random.Range(30f, 70f) : Random.Range(70f, 150f);
        
        newRune.affixes.Add(new RuneAffixData(randomStat, randomValue));

        return newRune;
    }
}