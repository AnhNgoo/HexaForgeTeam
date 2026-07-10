using UnityEngine;

public abstract class EnemyMinibossBehaviour : MonoBehaviour
{
    protected EnemyBase Enemy { get; private set; }
    public bool IsActionLocked { get; protected set; }

    protected virtual void Awake()
    {
        Enemy = GetComponent<EnemyBase>();
    }

    public virtual float ModifyIncomingDamage(float damage, Transform attacker)
    {
        return damage; // Mặc định không thay đổi sát thương
    }

    public virtual float ModifyAttackCooldown(float cooldown)
    {
        return cooldown;
    }

    public virtual float ModifyMoveSpeed(float speed)
    {
        return speed;
    }

    public virtual float ModifyDetectionRange(float range)
    {
        return range;
    }

    public virtual void ResetBehaviour()
    {
        IsActionLocked = false;
    }

    protected virtual void OnDisable()
    {
        ResetBehaviour();
    }

}
