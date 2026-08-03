using UnityEngine;

public class EnemyPoiseSystem : MonoBehaviour
{
    private EnemyBase _enemyBase;
    private float currentPoise;
    public float CurrentPoiseDamage => currentPoise;

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        currentPoise = 0f; // Khởi tạo poise ban đầu
    }

    public void TakePoiseDamage(float poiseDamage)
    {
        if (_enemyBase.StateMachine.CurrentState == _enemyBase.StateMachine.EnemyStaggerState)
        {
            Debug.Log($"{gameObject.name} đang trong trạng thái Stagger, không nhận thêm sát thương poise.");
            return; // Nếu đang trong trạng thái Stagger, không nhận thêm sát thương poise
        }

        currentPoise += poiseDamage;
        Debug.Log($"{gameObject.name} đã bị đánh, sát thương poise: {poiseDamage}, Poise hiện tại: {currentPoise}/{_enemyBase.Data.maxPoise}");
        if (currentPoise >= _enemyBase.Data.maxPoise)
        {
            // Gọi sự kiện vỡ trạng thái
            _enemyBase.EventManager.CallStagger();
        }
    }

    public void ResetPoise()
    {
        currentPoise = 0f;
        Debug.Log($"{gameObject.name} - Poise đã được reset.");
    }
    public void RecoverPoise(float amount)
    {
        if (amount <= 0f || currentPoise <= 0f) return;
        currentPoise = Mathf.Max(0f, currentPoise - amount);
    }

}
