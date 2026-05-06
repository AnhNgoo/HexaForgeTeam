using System;
using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    private EnemyBase _enemyBase;

    // Kênh báo bị đánh
    public event Action<float> OnTakeDamage;
    //Kênh báo tử
    public event Action OnDead;
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyEventManager đã được khởi tạo!");
    }

    public void CallTakeDamage(float damageAmount)
    {
        OnTakeDamage?.Invoke(damageAmount);
    }

    public void CallDead()
    {
        OnDead?.Invoke();
    }
}
