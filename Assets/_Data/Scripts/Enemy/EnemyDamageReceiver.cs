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
        finalDamage = Mathf.Max(0, rawDamage - _enemyBase.enemyData.maxDefense);
        _enemyBase.Heath.TakeDamage(finalDamage);

        //Gọi hệ thống poise để xử lý sát thương poise
        _enemyBase.PoiseSystem.TakePoiseDamage(poiseDamage);
    }
}
