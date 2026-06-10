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
        //To_Do: Tính toán sát thương cuối cùng dựa trên các yếu tố như phòng thủ, trạng thái, v.v.
        finalDamage = Mathf.Max(0, rawDamage - _enemyBase.Data.maxDefense);
        _enemyBase.Health.TakeDamage(finalDamage);

        //Gọi hệ thống poise để xử lý sát thương poise
        _enemyBase.PoiseSystem.TakePoiseDamage(poiseDamage);

        if (_enemyBase.Detection.CurrentTarget == null)
        {
            Transform attacker = GameObject.FindGameObjectWithTag("Player").transform; //Tìm game object có tag "Player" để lấy reference đến attacker, có thể dùng để xác định hướng tấn công và các hiệu ứng liên quan đến vị trí của attacker
            _enemyBase.Detection.ConfirmTarget(attacker); //Ép phát hiện attacker khi bị tấn công, có thể dùng để đảm bảo rằng Enemy sẽ phản ứng ngay lập tức khi bị tấn công mà không cần phải chờ đến lần kiểm tra phát hiện tiếp theo
        }
    }
}
