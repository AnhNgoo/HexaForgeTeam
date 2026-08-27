using System.Collections.Generic;
using UnityEngine;

public class DormantPowerInteractable : InteractBase
{
    [Header("Reward")]
    [SerializeField] private BossRewardTableSO rewardTable;

    [Header("Visual")]
    [SerializeField] private GameObject idleVisual;
    [SerializeField] private Vector3 vfxOffset = new(0f, 0.5f, 0f);
    [SerializeField]
    private PoolType dropVfx = PoolType.DormantPowerDropVFX;
    [SerializeField]
    private PoolType flickerVfx = PoolType.DormantPowerFlickerVFX;
    [SerializeField]
    private PoolType pickupVfx = PoolType.DormantPowerPickupVFX;

    private List<BossRewardDataSO> rolledRewards = new();
    private bool isClaimed;

    public override string InteractionName => "Nhận Dormant Power";

    public void Initialize(BossRewardTableSO table)
    {
        rewardTable = table;
        isClaimed = false;
        rolledRewards.Clear();

        if (idleVisual != null)
            idleVisual.SetActive(true);

        if (rewardTable != null)
            rolledRewards = rewardTable.RollRewards();

        if (rolledRewards.Count == 0)
        {
            Debug.LogError($"{name}: Reward table không tạo được card nào.");
            return;
        }

        PlayVfx(dropVfx);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        bool wasInRange = playerInRange;
        base.OnTriggerEnter(other);

        if (!wasInRange && playerInRange && !isClaimed && other.CompareTag("Player"))
        {
            PlayVfx(flickerVfx);
        }
    }

    protected override void InteractAction()
    {
        OpenRewardMenu();
    }

    private void OpenRewardMenu()
    {
        if (isClaimed || rolledRewards.Count == 0)
            return;

        UIManager.Instance?.ChangeMenu(MenuType.DormantPowerMenu, new DormantPowerMenuData(rolledRewards, ClaimReward, OpenWeaponReplacement));
    }

    private bool OpenWeaponReplacement(BossRewardDataSO reward)
    {
        WeaponInventorySystem inventory = WeaponInventorySystem.Instance;

        if (isClaimed || reward?.Weapon == null || inventory == null || inventory.CheckEmptyWeaponSlots() || UIManager.Instance == null)
        {
            return false;
        }

        UIManager.Instance.ChangeMenu(MenuType.InventoryMenu, new WeaponReplacementMenuData(reward.Weapon, slotIndex => ReplaceRewardWeapon(reward, slotIndex), OpenRewardMenu));

        return true;
    }

    private bool ReplaceRewardWeapon(BossRewardDataSO reward, int slotIndex)
    {
        if (isClaimed || reward?.Weapon == null || WeaponInventorySystem.Instance == null)
        {
            return false;
        }

        bool replaced = WeaponInventorySystem.Instance.TryReplaceWeapon(slotIndex, reward.Weapon);

        return replaced && CompleteClaim();
    }

    private bool CompleteClaim()
    {
        if (isClaimed) return false;
        isClaimed = true;
        InteractionManager.Instance?.UnregisterInteractable(this);
        EventManager.Notify(GameEvent.OnHidePickUpItemPanel);

        if (TryGetComponent(out Collider interactionCollider))
        {
            interactionCollider.enabled = false;
        }

        if (idleVisual != null) idleVisual.SetActive(false);
        PlayVfx(pickupVfx);
        UIManager.Instance?.ChangeMenu(MenuType.GameplayMenu);
        Destroy(gameObject);
        return true;
    }

    private bool ClaimReward(BossRewardDataSO reward)
    {
        if (isClaimed || reward == null || character == null)
            return false;

        if (!reward.Grant(character))
        {
            Debug.LogWarning($"Không thể nhận reward {reward.name}.");
            return false;
        }

        return CompleteClaim();
    }

    private void PlayVfx(PoolType poolType)
    {
        if (poolType == PoolType.None || ObjectPooling.Instance == null)
            return;

        ObjectPooling.Instance.SpawnFromPool(poolType, transform.position + vfxOffset, Quaternion.identity);
    }

    private void OnDestroy()
    {
        InteractionManager.Instance?.UnregisterInteractable(this);
    }

    public override void ResetInteraction()
    {
        // Dormant Power hiện được Instantiate/Destroy, không pool root.
    }
}
