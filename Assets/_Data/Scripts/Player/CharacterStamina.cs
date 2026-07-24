using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaData
{
    public float MaxStamina;
    public float CurrentStamina;
    public bool fullRegen;
}
public class CharacterStamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 0;
    public float MaxStamina => maxStamina;
    [SerializeField] private float currentStamina;
    public float CurrentStamina => currentStamina;

    private CharacterBase characterBase;
    private StaminaData staminaData = new StaminaData();

    public void Init(CharacterBase characterBase)
    {
        this.characterBase = characterBase;
    }

    private void Update()
    {
        if (characterBase == null)
            return;

        if (currentStamina < maxStamina)
        {
            currentStamina += characterBase.CharacterStat.Stats.staminaRegen * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            staminaData.CurrentStamina = currentStamina;
            staminaData.MaxStamina = maxStamina;
            staminaData.fullRegen = false;

            EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
        }
    }

    public void SetMaxStamina(float maxStamina, bool fullRegen = true)
    {
        this.maxStamina = maxStamina;
        if (currentStamina > maxStamina)
            currentStamina = maxStamina;
        if (fullRegen)
            currentStamina = maxStamina;

        staminaData.MaxStamina = maxStamina;
        staminaData.CurrentStamina = currentStamina;
        staminaData.fullRegen = fullRegen;
        EventManager.Notify(GameEvent.OnUpdateMaxStamina, staminaData);
    }

    public void AddStamina(float amount)
    {
        currentStamina += amount;
        if (currentStamina > maxStamina)
            currentStamina = maxStamina;

        staminaData.CurrentStamina = currentStamina;
        staminaData.MaxStamina = maxStamina;
        staminaData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
    }

    public void SubtractStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0)
            currentStamina = 0;

        staminaData.CurrentStamina = currentStamina;
        staminaData.MaxStamina = maxStamina;
        staminaData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
    }

    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void AddStaminaOverTime()
    {
        if (characterBase == null)
            return;

        if (currentStamina < maxStamina)
        {
            currentStamina += characterBase.CharacterStat.Stats.staminaRegen * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            staminaData.MaxStamina = maxStamina;
            staminaData.CurrentStamina = currentStamina;
            staminaData.fullRegen = false;
            EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
        }
    }

    public void SubtractStaminaOverTime(float amount)
    {
        currentStamina -= amount * Time.deltaTime;
        if (currentStamina < 0)
            currentStamina = 0;

        staminaData.MaxStamina = maxStamina;
        staminaData.CurrentStamina = currentStamina;
        staminaData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
    }
}
