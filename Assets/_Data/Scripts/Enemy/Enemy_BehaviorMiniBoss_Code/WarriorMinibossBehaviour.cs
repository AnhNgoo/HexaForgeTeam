using UnityEngine;

public class WarriorMinibossBehaviour : EnemyMinibossBehaviour
{
    [Header("Shield Guard")]
    [SerializeField, Range(0f, 180f)] private float guardAngle = 120f;
    [SerializeField, Range(0f, 1f)] private float frontDamageReduction = 0.5f;

    [Header("Shield Stance")]
    [SerializeField, Range(0f, 1f)] private float stanceTriggerHealthRatio = 0.7f;
    [SerializeField] private float stanceDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float stanceDamageReduction = 0.8f;
    [SerializeField] private float stanceTurnSpeed = 360f;
    [SerializeField] private string stanceAnimation = "Melee_Blocking";

    private bool _stanceTriggered;
    private bool _stanceActive;
    private float _stanceEndTime;

    private void Update()
    {
        if (Enemy == null || Enemy.Health.CurrentHealth <= 0f) return;


        bool isStaggered = Enemy.StateMachine.CurrentState == Enemy.StateMachine.EnemyStaggerState;

        if (!_stanceTriggered && !isStaggered)
        {
            float healthRatio = Enemy.Health.CurrentHealth / Enemy.Data.maxHealth;

            if (healthRatio <= stanceTriggerHealthRatio)
            {
                BeginShieldStance();
            }
        }

        if (!_stanceActive) return;

        Enemy.Locomotion.StopMoving();
        FaceTarget();

        if (Time.time >= _stanceEndTime)
            EndShieldStance();
    }

    public override float ModifyIncomingDamage(float damage, Transform attacker)
    {
        if (damage <= 0f) return damage;

        if (_stanceActive)
            return damage * (1f - stanceDamageReduction);

        if (!IsAttackerInFront(attacker))
            return damage;

        return damage * (1f - frontDamageReduction);
    }

    private bool IsAttackerInFront(Transform attacker)
    {
        if (attacker == null) return false;

        Vector3 direction = attacker.position - Enemy.MyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return true;

        float angle = Vector3.Angle(Enemy.MyTransform.forward, direction.normalized);

        return angle <= guardAngle * 0.5f;
    }

    private void BeginShieldStance()
    {
        _stanceTriggered = true;
        _stanceActive = true;
        IsActionLocked = true;
        _stanceEndTime = Time.time + stanceDuration;

        Enemy.Locomotion.StopMoving();
        Enemy.Combat.ForceCloseHitbox();

        Animator animator = Enemy.AnimatorController.Animator;

        if (animator != null && !string.IsNullOrEmpty(stanceAnimation))
            animator.CrossFadeInFixedTime(stanceAnimation, 0.1f);
    }

    private void EndShieldStance()
    {
        _stanceActive = false;
        IsActionLocked = false;

        Enemy.AnimatorController.PlayAnimation(Enemy.AnimatorController.IdleHash);
    }

    private void FaceTarget()
    {
        Transform target = Enemy.Detection.CurrentTarget;
        if (target == null) return;

        Vector3 direction = target.position - Enemy.MyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        Enemy.MyTransform.rotation = Quaternion.RotateTowards(
            Enemy.MyTransform.rotation,
            targetRotation,
            stanceTurnSpeed * Time.deltaTime
        );
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        _stanceTriggered = false;
        _stanceActive = false;
        _stanceEndTime = 0f;
    }
}
