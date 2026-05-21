using UnityEngine;

public class EnemyState_Dead : EnemyState
{
    public EnemyState_Dead(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Dead.");
        // To_Do: Thực hiện các hành động khi vào trạng thái Dead, ví dụ: phát animation chết, vô hiệu hoá collider, v.v.
        _enemyBase.Locomotion.StopMoving();
        _enemyBase.MainCollider.enabled = false; // Vô hiệu hoá collider để không còn tương tác vật lý
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.DieHash); // Phát animation chết
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // Trong trạng thái chết, có thể không cần logic gì đặc biệt, hoặc có thể thêm hiệu ứng sau khi chết nếu muốn
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log($"{_enemyBase.gameObject.name} đã rời khỏi trạng thái Dead.");
        // To_Do: Nếu có logic nào cần thực hiện khi rời khỏi trạng thái Dead (thường là không), thì thực hiện ở đây
    }
}
