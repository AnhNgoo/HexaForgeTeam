using UnityEngine;

public class EnemyState_Block : EnemyState
{
    private float _blockEndTime;
    private bool _usedShieldBash;

    public EnemyState_Block(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();

        _usedShieldBash = false;
        _blockEndTime =
            Time.time + _enemyBase.Guard.GetGuardDuration();

        _enemyBase.Guard.BeginGuard();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (_enemyBase.MinibossBehaviour != null && _enemyBase.MinibossBehaviour.IsActionLocked)
        {
            _enemyBase.Locomotion.StopMoving();
            return;
        }

        Transform target = _enemyBase.Detection.CurrentTarget;

        if (target == null)
        {
            _enemyBase.StateMachine.ChangeState(
                _enemyBase.StateMachine.EnemyIdleState
            );
            return;
        }

        if (!_enemyBase.Detection.IsPointInLeash(target.position))
        {
            _enemyBase.Detection.ForceLoseTarget();
            _enemyBase.StateMachine.ChangeState(
                _enemyBase.StateMachine.EnemySuspicionState
            );
            return;
        }

        FaceTarget(target);

        float distance = Vector3.Distance(
            _enemyBase.MyTransform.position,
            target.position
        );

        if (!_usedShieldBash &&
            _enemyBase.Guard.CanShieldBash(distance))
        {
            _usedShieldBash = true;
            _enemyBase.Guard.StartShieldBash();
        }

        if (Time.time >= _blockEndTime &&
            !_enemyBase.Guard.IsBashing)
        {
            _enemyBase.StateMachine.ChangeState(
                _enemyBase.StateMachine.EnemyChaseState
            );
        }
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Guard.EndGuard();
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction =
            target.position - _enemyBase.MyTransform.position;

        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        _enemyBase.MyTransform.rotation = Quaternion.Slerp(
            _enemyBase.MyTransform.rotation,
            targetRotation,
            Time.deltaTime * 4f
        );
    }
}
