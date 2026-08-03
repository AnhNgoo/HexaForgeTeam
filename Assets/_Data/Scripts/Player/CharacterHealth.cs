using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthData
{
    public float MaxHealth;
    public float CurrentHealth;
    public bool fullHeal;
}
public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 0;
    public float MaxHealth => maxHealth;
    [SerializeField] private float currentHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;
    private HealthData healthData = new HealthData();

    public void Init(float maxHealth)
    {
        SetMaxHealth(maxHealth, true);
    }

    public void ResetHealth()
    {
        SetMaxHealth(maxHealth, true);
    }
    public void SetMaxHealth(float maxHealth, bool fullHeal = true)
    {
        float normalizedMaxHealth = Mathf.Max(1f, maxHealth);

        this.maxHealth = normalizedMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, normalizedMaxHealth);
        if (fullHeal)
            currentHealth = normalizedMaxHealth;

        healthData.MaxHealth = normalizedMaxHealth;
        healthData.CurrentHealth = currentHealth;
        healthData.fullHeal = fullHeal;
        EventManager.Notify(GameEvent.OnUpdateMaxHealth, healthData);
    }
    public void AddHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        healthData.MaxHealth = maxHealth;
        healthData.CurrentHealth = currentHealth;
        healthData.fullHeal = false;
        EventManager.Notify(GameEvent.OnUpdateHealth, healthData);
    }

    public void SubtractHealth(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        healthData.MaxHealth = maxHealth;
        healthData.CurrentHealth = currentHealth;
        healthData.fullHeal = false;
        EventManager.Notify(GameEvent.OnUpdateHealth, healthData);
    }
}