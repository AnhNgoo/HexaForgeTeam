using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dusklight : InteractBase
{

    public override string InteractionName => "Level Up"; // Lên cấp level
    public override void ResetInteraction()
    {

    }

    protected override void InteractAction()
    {
        // Lấy chi phí lên cấp dựa trên cấp độ tiếp theo của nhân vật
        int levelUpCostNext = character.CharacterLevel.StatGainedLevelUp.GetLevelUpCost(character.CharacterLevel.CurrentLevel + 1);
        if (!GoldManager.Instance.HasEnoughGold(levelUpCostNext)) // Bỏ qua nếu không đủ vàng
        {
            NotifyUI notifyUI = ObjectPooling.Instance.SpawnFromPool(PoolType.NotifyUI, transform.position, Quaternion.identity)?.GetComponent<NotifyUI>();
            if (notifyUI != null)
            {
                notifyUI.SetDescription("Not enough gold to level up!"); // Không đủ vàng để lên cấp
            }
            return;
        }

        if (character.CharacterLevel.CurrentLevel >= character.CharacterLevel.MaxLevel) // Bỏ qua nếu đã đạt cấp tối đa
        {
            NotifyUI notifyUI = ObjectPooling.Instance.SpawnFromPool(PoolType.NotifyUI, transform.position, Quaternion.identity)?.GetComponent<NotifyUI>();
            if (notifyUI != null)
            {
                notifyUI.SetDescription("Already at max level!"); // Đã đạt cấp tối đa
            }
            return;
        }

        GoldManager.Instance.RemoveGold(levelUpCostNext); // Trừ vàng
        character.CharacterLevel.LevelUp(); // Lên cấp
        ObjectPooling.Instance.SpawnFromPool(PoolType.LevelUpEffect, transform.position, Quaternion.identity); // Hiệu ứng lên cấp
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (character == null)
            {
                character = PlayerManager.Instance.CurrentCharacterBase;
            }
            playerInRange = true;
            InteractionManager.Instance?.RegisterInteractable(this);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (character != null &&
                PlayerManager.Instance.CurrentCharacterBase == character)
            {
                character = null;
            }
            playerInRange = false;
            InteractionManager.Instance?.UnregisterInteractable(this);
        }
    }
}
