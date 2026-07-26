using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterRecovery : MonoBehaviour
{
    [SerializeField] private int maxRecoveryBottle = 3;
    [SerializeField] private int recoveryBottle = 0;
    public int RecoveryBottle => recoveryBottle;
    [SerializeField] private float healPercent = 36;

    private CharacterBase character;

    public void Init(CharacterBase character)
    {
        this.character = character;
        AddBottle(3); // Khởi tạo số lượng bình hồi máu ban đầu
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, recoveryBottle);
    }

    [Button("Add Recovery Bottle")]
    public void AddBottle(int amount = 1)
    {
        recoveryBottle = Mathf.Min(recoveryBottle + amount, maxRecoveryBottle);
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, recoveryBottle);
    }

    public void ResetBottle()
    {
        recoveryBottle = 0;
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, recoveryBottle);
    }

    // Chỉnh phần trăm healPercent
    [Button("Set Heal Percent")]
    public void SetHealPercent(float percent)
    {
        healPercent = percent;
    }

    public void UseRecoveryBottle()
    {
        if (recoveryBottle <= 0)
            return;

        recoveryBottle--;
        float healAmount = character.CharacterHealth.MaxHealth * healPercent / 100f;
        character.CharacterHealth.AddHealth(healAmount);
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, recoveryBottle);
    }
}
