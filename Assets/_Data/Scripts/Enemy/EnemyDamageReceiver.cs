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
        Debug.Log($"{gameObject.name} - EnemyDamageReceiver đã được khởi tạo!");
    }

    public void TakeHit(float rawDamage, float poiseDamage)
    {
        if (_enemyBase.Health.CurrentHealth <= 0) return; //Nếu đã chết thì không nhận thêm sát thương

        bool isStaggered = _enemyBase.StateMachine.CurrentState == _enemyBase.StateMachine.EnemyStaggerState;

        if (isStaggered)
        {
            finalDamage = rawDamage; //Nếu đang bị stagger thì bỏ qua phòng thủ, nhận sát thương gốc
            Debug.Log($"{_enemyBase.gameObject.name} đang bị stagger, bỏ qua phòng thủ và nhận sát thương gốc: {finalDamage}");
        }
        else
        {
            finalDamage = Mathf.Max(0, rawDamage - _enemyBase.Data.maxDefense);
        }
        _enemyBase.Health.TakeDamage(finalDamage);

        if (_enemyBase.Health.CurrentHealth <= 0) return; //Nếu đã chết sau khi nhận sát thương thì không cần xử lý poise

        if (isStaggered)
        {
            _enemyBase.StateMachine.EnemyStaggerState.OnHitDuringStagger(); //Nếu đang bị stagger và bị đánh trúng, thì gọi phương thức OnHitDuringStagger để xử lý logic đặc biệt khi bị đánh trúng trong trạng thái stagger (ví dụ như reset thời gian stagger, tăng thời gian stagger, hoặc các hiệu ứng đặc biệt khác)
        }
        else
        {
            _enemyBase.PoiseSystem.TakePoiseDamage(poiseDamage); //Nếu không đang bị stagger thì vẫn nhận sát thương poise như bình thường, có thể dẫn đến việc bị stagger nếu poise giảm xuống dưới ngưỡng
        }

        if (_enemyBase.Detection.CurrentTarget == null)
        {
            Transform attacker = GameObject.FindGameObjectWithTag("Player").transform; //Tìm game object có tag "Player" để lấy reference đến attacker, có thể dùng để xác định hướng tấn công và các hiệu ứng liên quan đến vị trí của attacker
            _enemyBase.Detection.ReportDamageHit(attacker); //Ép phát hiện attacker khi bị tấn công, có thể dùng để đảm bảo rằng Enemy sẽ phản ứng ngay lập tức khi bị tấn công mà không cần phải chờ đến lần kiểm tra phát hiện tiếp theo
        }
    }
}
