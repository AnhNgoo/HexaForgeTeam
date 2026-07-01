using UnityEngine;

public class ShadeMinibossBehaviour : EnemyMinibossBehaviour
{
    [SerializeField] private float hunterRange = 8f;
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float pursuitStoppingDistance = 1.5f;
    [SerializeField] private float pursuitDuration = 1.2f;

    private float _pursuitEndTime;

    private void Update()
    {
        if (Enemy == null || Enemy.Health.CurrentHealth <= 0f) return;

        if (Time.time < _pursuitEndTime)
        {
            Transform target = Enemy.Detection.CurrentTarget;
            if (target == null) return;

            Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed * speedMultiplier);
            Enemy.Locomotion.MoveToTarget(target.position, pursuitStoppingDistance);
        }
    }

    public override float ModifyMoveSpeed(float speed)
    {
        Transform target = Enemy.Detection.CurrentTarget;
        if (target == null) return speed;

        float distance = Vector3.Distance(Enemy.MyTransform.position, target.position);
        return distance > hunterRange ? speed * speedMultiplier : speed;
    }

    public void NotifyShadowBindSuccess()
    {
        _pursuitEndTime = Time.time + pursuitDuration;
    }
}
