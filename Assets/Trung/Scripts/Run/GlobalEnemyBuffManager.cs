using UnityEngine;
using System.Collections.Generic;

public class GlobalEnemyBuffManager : MonoBehaviour
{
    public static GlobalEnemyBuffManager Instance;

    private readonly HashSet<EnemyBase> buffedEnemies = new HashSet<EnemyBase>();
    private float scanTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer >= 0.3f)
        {
            scanTimer = 0f;
            ScanAndBuffEnemies();
        }
    }

    private void OnDisable()
    {
        buffedEnemies.Clear();
    }

    private void ScanAndBuffEnemies()
    {
        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemies)
        {
            if (enemy != null && !buffedEnemies.Contains(enemy))
            {
                buffedEnemies.Add(enemy);
                ApplyGlobalBuffToEnemy(enemy);
            }
        }
    }

    private void ApplyGlobalBuffToEnemy(EnemyBase enemy)
    {
        if (enemy == null || enemy.Data == null) return;

        float difficultyMultiplier = 1.0f;
        ActiveRunBuffs buffs = new ActiveRunBuffs();

        if (RunManager.Instance != null)
        {
            difficultyMultiplier = RunManager.Instance.CurrentRewardMultiplier;
            buffs = RunManager.Instance.ActiveBuffs;
        }

        // 1. TĂNG MÁU QUÁI THEO HỆ SỐ ĐỘ KHÓ WAGER
        float originalMax = enemy.Data.maxHealth;
        float finalMax = originalMax;

        if (difficultyMultiplier > 1.0f)
        {
            finalMax = originalMax * difficultyMultiplier;

            if (enemy.Health != null)
            {
                enemy.Health.SetMaxHealthDirectly(finalMax);
            }
        }

        // IN DEBUG LOG CHI TIẾT
        Debug.Log($"<color=cyan><b>[GLOBAL BUFF]</b> Quái: <b>{enemy.gameObject.name}</b> | Độ khó: <b>x{difficultyMultiplier}</b> | HP: {originalMax} ➔ <color=#00FF00>{finalMax}</color> | Buffs: [Gold: {buffs.hasGoldBuff}, ATK: {buffs.hasAtkBuff}, Revive: {buffs.hasReviveBuff}]</color>");

        // 2. LẮNG NGHE SỰ KIỆN NHẬN ĐÒN & CHẾT RƠI VÀNG
        if (enemy.EventManager != null)
        {
            // Bồi thêm 15% Sát thương khi người chơi bật Buff ATK
            enemy.EventManager.OnTakeDamage += (incomingDamage) => HandlePlayerAttackBuff(enemy, incomingDamage);

            // Thưởng thêm 30% Vàng khi quái chết nếu bật Buff Gold
            enemy.EventManager.OnDead += () => HandleEnemyDefeatedGold(enemy);
        }
    }

    private void HandlePlayerAttackBuff(EnemyBase enemy, float incomingDamage)
    {
        if (enemy == null || enemy.Health == null || RunManager.Instance == null) return;

        if (RunManager.Instance.ActiveBuffs.hasAtkBuff && incomingDamage > 0f)
        {
            float bonusDamage = incomingDamage * 0.15f; // +15% Sát thương phụ trội từ Power Elixir
            if (bonusDamage > 0f)
            {
                enemy.Health.TakeDamage(bonusDamage);
                Debug.Log($"<color=red><b>[ATK BUFF +15%]</b> Gây thêm <b>+{bonusDamage:F1}</b> DMG lên <b>{enemy.gameObject.name}</b> (Gốc: {incomingDamage:F1})</color>");
            }
        }
    }

    private void HandleEnemyDefeatedGold(EnemyBase enemy)
    {
        if (enemy == null || RunManager.Instance == null) return;

        if (RunManager.Instance.ActiveBuffs.hasGoldBuff && GoldManager.Instance != null)
        {
            int baseDropGold = 0;

            if (enemy.Data != null)
            {
                int minGold = enemy.Data.minGoldReward;
                int maxGold = enemy.Data.maxGoldReward;

                if (maxGold > minGold)
                {
                    baseDropGold = Random.Range(minGold, maxGold + 1);
                }
                else
                {
                    baseDropGold = Mathf.Max(minGold, maxGold);
                }
            }

            if (baseDropGold <= 0)
            {
                baseDropGold = enemy.Data != null && enemy.Data.isBoss ? 150 : 50;
            }

            int bonusGold = Mathf.RoundToInt(baseDropGold * 0.3f);
            if (bonusGold > 0)
            {
                GoldManager.Instance.AddGold(bonusGold);
                Debug.Log($"<color=yellow><b>[GOLD BUFF +30%]</b> Nhận thêm <b>+{bonusGold} Vàng</b> (+30% của {baseDropGold}) từ <b>{enemy.gameObject.name}</b>!</color>");
            }
        }
    }
}