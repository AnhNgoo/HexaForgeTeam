using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private float currentHealth;
    private float maxHealthOverride = -1f;
    public float CurrentHealth => currentHealth;
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        currentHealth = _enemyBase.Data.maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        _enemyBase.EventManager.CallTakeDamage(damageAmount);
        if (currentHealth <= 0)
        {
            _enemyBase.EventManager.CallDead();
            Debug.Log($"{gameObject.name} đã chết.");
        }
        Debug.Log($"{gameObject.name} đã bị đánh, sát thương: {damageAmount}, HP còn lại: {currentHealth}");
    }

    public void ResetHealth()
    {
        currentHealth = _enemyBase.Data.maxHealth;
    }

    public void LoadSavedHealth(float savedHealth)
    {
        //Nếu savedHealth < 0 thì không có giá trị đã lưu nào hợp lệ, đặt currentHealth về maxHealth, ngược lại thì sử dụng giá trị đã lưu
        currentHealth = (savedHealth < 0) ? _enemyBase.Data.maxHealth : savedHealth; //Nếu savedHealth < 0 thì không có giá trị đã lưu nào hợp lệ, đặt currentHealth về maxHealth, ngược lại thì sử dụng giá trị đã lưu
        Debug.Log($"{gameObject.name} đã load lượng máu đã lưu: {currentHealth}");
    }
    public void SetMaxHealthDirectly(float newMaxHealth)
    {
        maxHealthOverride = newMaxHealth;
        currentHealth = newMaxHealth;
    }
}
