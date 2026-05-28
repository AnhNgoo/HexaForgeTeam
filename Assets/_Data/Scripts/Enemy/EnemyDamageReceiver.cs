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
        _enemyBase.Heath.TakeDamage(finalDamage);

        //Gọi hệ thống poise để xử lý sát thương poise
        _enemyBase.PoiseSystem.TakePoiseDamage(poiseDamage);

        //Gọi đồng đội xung quanh khi bị tấn công
        _enemyBase.Detection.AlertNearbyAllies(GameObject.FindGameObjectWithTag("Player").transform); //Gọi hàm cảnh báo đồng bọn khi bị tấn công, có thể mở rộng sau này để truyền thông tin về mục tiêu cho các Enemy khác trong bán kính cảnh báo thay vì chỉ đơn giản là truyền vị trí của player (ví dụ: truyền trạng thái hiện tại của player như đang tấn công, đang phòng thủ, v.v.) để đồng bọn có thể phản ứng phù hợp hơn thay vì chỉ đơn giản là phát hiện mục tiêu như nhau với cùng một trạng thái.
    }
}
