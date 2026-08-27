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
        AddStaminaOverTime();
    }

    public void ResetStamina()
    {
        SetMaxStamina(maxStamina, true);
    }
    public void SetMaxStamina(float maxStamina, bool fullRegen = true)
    {
        Debug.Log($"SetMaxStamina called with maxStamina = {maxStamina}, fullRegen = {fullRegen}");
        float normalizedMaxStamina = Mathf.Max(1f, maxStamina);

        this.maxStamina = normalizedMaxStamina;
        currentStamina = Mathf.Clamp(currentStamina, 0, normalizedMaxStamina);
        if (fullRegen)
            currentStamina = normalizedMaxStamina;

        staminaData.MaxStamina = normalizedMaxStamina;
        staminaData.CurrentStamina = currentStamina;
        staminaData.fullRegen = fullRegen;
        EventManager.Notify(GameEvent.OnUpdateMaxStamina, staminaData);
    }

    public void AddStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        staminaData.CurrentStamina = currentStamina;
        staminaData.MaxStamina = maxStamina;
        staminaData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
    }

    public void SubtractStamina(float amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.MapType == MapType.Lobby)
        {
            return;
        }

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

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
            currentStamina += characterBase.CharacterStat.finalStats.staminaRegen * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            staminaData.MaxStamina = maxStamina;
            staminaData.CurrentStamina = currentStamina;
            staminaData.fullRegen = false;
            EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
        }
    }

    public void SubtractStaminaOverTime(float amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.MapType == MapType.Lobby)
        {
            return;
        }

        currentStamina -= amount * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        staminaData.MaxStamina = maxStamina;
        staminaData.CurrentStamina = currentStamina;
        staminaData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateStamina, staminaData);
    }
}
