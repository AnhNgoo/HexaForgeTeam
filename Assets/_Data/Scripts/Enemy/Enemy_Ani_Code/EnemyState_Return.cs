using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyState_Return : EnemyState
{
    public EnemyState_Return(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed); //Đi từ từ vê vị trí ban đầu
        _enemyBase.Locomotion.MoveToTarget(_enemyBase.SpawnOrigin); //Di chuyển về vị trí xuất hiện ban đầu
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash); // Phát animation chạy khi vào trạng thái này

        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Return.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (_enemyBase.Detection.CurrentTarget != null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState); //Nếu đang trên đường về mà phát hiện mục tiêu thì chuyển sang trạng thái Chase
            return;
        }

        Vector3 myPos = new Vector3(_enemyBase.MyTransform.position.x, 0, _enemyBase.MyTransform.position.z);
        Vector3 originPos = new Vector3(_enemyBase.SpawnOrigin.x, 0, _enemyBase.SpawnOrigin.z);

        if (Vector3.Distance(myPos, originPos) <= 0.5f) //Nếu đã về gần vị trí xuất hiện ban đầu thì chuyển sang trạng thái Patrol
        {
            _enemyBase.Locomotion.StopMoving(); //Dừng lại khi đã về gần vị trí xuất hiện ban đầu để tránh việc Enemy vẫn tiếp

            _enemyBase.MyTransform.rotation = Quaternion.Slerp(_enemyBase.MyTransform.rotation, _enemyBase.SpawnRotation, Time.deltaTime * 8f); //Quay về hướng gốc khi đã về gần vị trí xuất hiện ban đầu để tạo hiệu ứng tương tác và tăng tính chân thực của Enemy, có thể điều chỉnh tốc độ quay nếu cần thiết

            if (Quaternion.Angle(_enemyBase.MyTransform.rotation, _enemyBase.SpawnRotation) < 5f) //Nếu đã quay về gần hướng gốc thì chuyển sang trạng thái Patrol, có thể điều chỉnh giá trị này nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể không cần quay về hướng gốc và có thể chuyển sang trạng thái Patrol ngay khi đã về gần vị trí xuất hiện ban đầu)
            {
                _enemyBase.MyTransform.rotation = _enemyBase.SpawnRotation; //Đặt chính xác hướng gốc để tránh lỗi quay không chính xác khi đã về gần vị trí xuất hiện ban đầu
                _enemyBase.StateMachine.ResetToDefaultState(); //Chuyển về trạng thái ban đầu (Idle hoặc Patrol tùy thiết lập) sau khi đã về gần vị trí xuất hiện ban đầu và đã quay về hướng gốc, có thể điều chỉnh lại logic này nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể luôn chuyển sang trạng thái Idle sau khi đã về gần vị trí xuất hiện ban đầu trong khi một số loại khác có thể luôn chuyển sang trạng thái Patrol)
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Locomotion.StopMoving();

        _enemyBase.Health.ResetHealth(); //Khi về trạng thái Return thì sẽ hồi phục máu, có thể điều chỉnh lại nếu muốn chỉ hồi một phần hoặc không hồi máu
        _enemyBase.PoiseSystem.ResetPoise(); //Khi về trạng thái Return thì sẽ reset Poise, có thể điều chỉnh lại nếu muốn chỉ reset một phần hoặc không reset Poise
        _enemyBase.ResetLeash(); //Khi về trạng thái Return thì sẽ reset dây xích, có thể điều chỉnh lại nếu muốn chỉ reset một phần hoặc không reset dây xích
    }
}
