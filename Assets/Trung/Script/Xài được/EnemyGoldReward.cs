using UnityEngine;

public class EnemyGoldReward : MonoBehaviour
{
    [Header("Phân Loại Enemy")]
    [SerializeField] private EnemyRewardType enemyRewardType;

    [Header("Gold Reward")]
    [SerializeField] private int minGoldReward;
    [SerializeField] private int maxGoldReward;

    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        if (enemyBase == null)
            return;

        enemyBase.EventManager.OnDead += RewardGold;
    }

    private void OnDisable()
    {
        if (enemyBase == null)
            return;

        enemyBase.EventManager.OnDead -= RewardGold;
    }

    private void RewardGold()
    {
        int goldReward = Random.Range(minGoldReward, maxGoldReward + 1);

        GoldManager.Instance?.AddGold(goldReward);

        Debug.Log($"{gameObject.name} rơi {goldReward} vàng");
    }
}