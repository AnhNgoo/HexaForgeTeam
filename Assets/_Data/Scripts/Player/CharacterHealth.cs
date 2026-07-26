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

    public void Init(float maxHealth)
    {
        SetMaxHealth(maxHealth, true);
    }

    public void SetMaxHealth(float maxHealth, bool fullHeal = true)
    {
        this.maxHealth = maxHealth;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        if (fullHeal)
            currentHealth = maxHealth;

        HealthData healthData = new HealthData
        {
            MaxHealth = maxHealth,
            CurrentHealth = currentHealth,
            fullHeal = fullHeal
        };
        EventManager.Notify(GameEvent.OnUpdateMaxHealth, healthData);
    }
    public void AddHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        HealthData healthData = new HealthData
        {
            MaxHealth = maxHealth,
            CurrentHealth = currentHealth,
            fullHeal = false
        };
        EventManager.Notify(GameEvent.OnUpdateHealth, healthData);
    }

    public void SubtractHealth(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;

        HealthData healthData = new HealthData
        {
            MaxHealth = maxHealth,
            CurrentHealth = currentHealth,
            fullHeal = false
        };
        EventManager.Notify(GameEvent.OnUpdateHealth, healthData);
    }
}
