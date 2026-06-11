using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 0;
    public float MaxHealth => maxHealth;
    [SerializeField] private float currentHealth;
    public float CurrentHealth => currentHealth;

    public void Init(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
    }


    public void AddHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void SubtractHealth(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;
    }
}
