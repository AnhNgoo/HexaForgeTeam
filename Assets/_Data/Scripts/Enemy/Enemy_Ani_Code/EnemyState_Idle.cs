using UnityEngine;

public class EnemyState_Idle : EnemyState
{
    public EnemyState_Idle(EnemyBase enemyBase) : base(enemyBase)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // To_Do: Thực hiện các hành động khi vào trạng thái Idle, ví dụ: phát animation Idle
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Idle.");
    }
}
