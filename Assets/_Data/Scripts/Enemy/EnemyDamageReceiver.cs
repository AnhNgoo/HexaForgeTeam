using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageReceiver : MonoBehaviour
{
    private EnemyBase _enemyBase;
    private float finalDamage;

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
    }

    public void TakeHit(float rawDamage, float poiseDamage)
    {
        TakeHit(rawDamage, poiseDamage, null);
    }

    public void TakeHit(float rawDamage, float poiseDamage, Transform attacker)
    {
        if (_enemyBase.Health.CurrentHealth <= 0) return; //Nếu đã chết thì không nhận thêm sát thương

        bool isStaggered = _enemyBase.StateMachine.CurrentState == _enemyBase.StateMachine.EnemyStaggerState;
        bool isSpecialActionLocked = _enemyBase.MinibossBehaviour != null && _enemyBase.MinibossBehaviour.IsActionLocked;

        if (!isStaggered && !isSpecialActionLocked && _enemyBase.Guard != null)
        {
            EnemyGuardResult guardResult = _enemyBase.Guard.TryBlock(rawDamage, poiseDamage, attacker, out float blockedDamage);

            if (guardResult != EnemyGuardResult.NotBlocked)
            {
                _enemyBase.Health.TakeDamage(blockedDamage);

                if (_enemyBase.Health.CurrentHealth <= 0f) return;

                if (guardResult == EnemyGuardResult.Broken)
                    _enemyBase.EventManager.CallStagger();
                else
                    _enemyBase.Guard.PlayBlockHit();

                ReportAttacker(attacker);
                return;
            }
        }

        float finalDamage = isStaggered ? rawDamage : Mathf.Max(0f, rawDamage - _enemyBase.Data.maxDefense);

        if (!isStaggered && _enemyBase.MinibossBehaviour != null)
        {
            finalDamage = _enemyBase.MinibossBehaviour.ModifyIncomingDamage(finalDamage, attacker);
        }

        _enemyBase.Health.TakeDamage(finalDamage);

        if (_enemyBase.Health.CurrentHealth <= 0f) return;

        if (isStaggered)
            _enemyBase.StateMachine.EnemyStaggerState.OnHitDuringStagger();
        else if (poiseDamage > 0f)
            _enemyBase.PoiseSystem.TakePoiseDamage(poiseDamage);

        ReportAttacker(attacker);
    }

    public void ReportAttacker(Transform attacker)
    {
        if (attacker == null) return;

        if (_enemyBase.Detection.CurrentTarget == null)
            _enemyBase.Detection.ReportDamageHit(attacker);
    }
}
