using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterRecovery : MonoBehaviour
{
    [SerializeField] private int maxRecoveryBottle = 7;
    [SerializeField] private int startRecoveryBottle = 3;
    public int StartRecoveryBottle => startRecoveryBottle;
    [SerializeField] private int currentRecoveryBottle = 0;
    public int CurrentRecoveryBottle => currentRecoveryBottle;
    [SerializeField] private float healPercent = 36;

    private CharacterBase character;
    // Số lượng bình máu tối đa đã nhặt hiện tại
    public int CurrentMaxRecoveryBottle { get; private set; } = 0;

    public void Init(CharacterBase character)
    {
        this.character = character;
        AddBottle(startRecoveryBottle); // Khởi tạo số lượng bình hồi máu ban đầu
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, currentRecoveryBottle);
    }

    public void ResetRecovery()
    {
        AddBottle(CurrentMaxRecoveryBottle);
    }

    [Button("Add Recovery Bottle")]
    public void AddBottle(int amount = 1)
    {
        currentRecoveryBottle = Mathf.Min(currentRecoveryBottle + amount, maxRecoveryBottle);
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, currentRecoveryBottle);

        CurrentMaxRecoveryBottle = currentRecoveryBottle;
    }

    public void ResetBottle()
    {
        currentRecoveryBottle = 0;
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, currentRecoveryBottle);
    }

    // Chỉnh phần trăm healPercent
    [Button("Set Heal Percent")]
    public void SetHealPercent(float percent)
    {
        healPercent = percent;
    }

    public void UseRecoveryBottle()
    {
        if (currentRecoveryBottle <= 0)
            return;

        currentRecoveryBottle--;
        float healAmount = character.CharacterHealth.MaxHealth * healPercent / 100f;
        character.CharacterHealth.AddHealth(healAmount);
        EventManager.Notify(GameEvent.OnUpdateRecoveryBottle, currentRecoveryBottle);
    }
}
