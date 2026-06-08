using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class EnemyState_Dead : EnemyState
{
    public EnemyState_Dead(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Dead.");

        _enemyBase.Locomotion.StopMoving();

        _enemyBase.Locomotion.SetAgentActive(false); //Vô hiệu hóa NavMeshAgent để tránh lỗi di chuyển sau khi chết

        if (_enemyBase.MainCollider != null)
        {
            _enemyBase.MainCollider.enabled = true;
        }
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.DieHash); // Phát animation chết

        StartDespawnTimer().Forget(); // Bắt đầu timer để despawn sau khi chết, sử dụng Forget() để chạy bất đồng bộ mà không cần chờ đợi kết quả
    }

    private async UniTaskVoid StartDespawnTimer()
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: _enemyBase.GetCancellationTokenOnDestroy()); // Chờ 5 giây trước khi despawn

            if (_enemyBase.MainCollider != null) _enemyBase.MainCollider.enabled = false; // Vô hiệu hóa collider để tránh tương tác sau khi chết

            _enemyBase.Despawn(); // Gọi phương thức despawn để trả Enemy về pool
        }
        catch (OperationCanceledException)
        {

        }
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
