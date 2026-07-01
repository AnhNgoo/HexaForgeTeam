using UnityEngine;
using System.Collections.Generic;

public class SkeletonMageMinibossBehaviour : EnemyMinibossBehaviour
{
    [SerializeField, Range(0f, 1f)] private float auraDamageReduction = 0.3f;
    [SerializeField] private float frenzyDuration = 5f;
    [SerializeField] private float frenzyAttackSpeedMultiplier = 1.5f;

    private readonly List<EnemyBase> _aliveMinions = new();
    private float _frenzyEndTime;

    public override float ModifyIncomingDamage(float damage, Transform attacker)
    {
        CleanupMinions();

        if (_aliveMinions.Count > 0)
            return damage * (1f - auraDamageReduction);

        return damage;
    }

    public override float ModifyAttackCooldown(float cooldown)
    {
        if (Time.time < _frenzyEndTime)
            return cooldown / frenzyAttackSpeedMultiplier;

        return cooldown;
    }

    public void RegisterMinion(EnemyBase minion)
    {
        if (minion == null || _aliveMinions.Contains(minion)) return;

        _aliveMinions.Add(minion);
        minion.EventManager.OnDead += OnMinionDead;
    }

    private void OnMinionDead()
    {
        CleanupMinions();

        if (_aliveMinions.Count == 0)
            _frenzyEndTime = Time.time + frenzyDuration;
    }

    private void CleanupMinions()
    {
        _aliveMinions.RemoveAll(m => m == null || m.Health.CurrentHealth <= 0f);
    }

    public void SetCastingLocked(bool locked)
    {
        IsActionLocked = locked;

        if (locked && Enemy != null)
            Enemy.Locomotion.StopMoving();
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        foreach (EnemyBase minion in _aliveMinions)
        {
            if (minion != null)
                minion.EventManager.OnDead -= OnMinionDead;
        }

        _aliveMinions.Clear();
        _frenzyEndTime = 0f;
    }
}
