using System;
using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    private EnemyBase _enemyBase;

    // Kênh báo bị đánh
    public event Action<float> OnTakeDamage;
    //Kênh báo tử
    public event Action OnDead;
    //Kênh báo vỡ trạng thái
    public event Action OnStagger;
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
    }

    public void CallTakeDamage(float damageAmount)
    {
        OnTakeDamage?.Invoke(damageAmount);
    }

    public void CallDead()
    {
        OnDead?.Invoke();
    }

    public void CallStagger()
    {
        OnStagger?.Invoke();
    }
}
