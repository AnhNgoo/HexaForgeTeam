using UnityEngine;

public class BurrowMinibossBehaviour : EnemyMinibossBehaviour
{
    [SerializeField] private float camouflageDetectRange = 7f;
    [SerializeField] private float frenzyDuration = 3f;
    [SerializeField] private float frenzyAttackSpeedMultiplier = 1.5f;

    private float _frenzyEndTime;

    public override float ModifyDetectionRange(float range)
    {
        bool isStationary = Enemy != null &&
            Enemy.StateMachine.CurrentState == Enemy.StateMachine.EnemyIdleState;

        return isStationary ? Mathf.Min(range, camouflageDetectRange) : range;
    }

    public override float ModifyAttackCooldown(float cooldown)
    {
        if (Time.time < _frenzyEndTime)
            return cooldown / frenzyAttackSpeedMultiplier;

        return cooldown;
    }

    public void NotifyBurrowEmerged()
    {
        _frenzyEndTime = Time.time + frenzyDuration;
    }

    public void SetActionLocked(bool locked)
    {
        IsActionLocked = locked;
    }
}
