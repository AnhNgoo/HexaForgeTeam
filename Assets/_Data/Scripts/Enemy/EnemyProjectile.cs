using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private Vector3 _direction;
    private bool _isLaunched = false;

    public void Launch(float damage, float speed, Vector3 direction)
    {
        _damage = damage;
        _speed = speed;
        _direction = direction.normalized;
        _isLaunched = true;

        transform.forward = _direction; // Xoay hướng viên đạn về hướng di chuyển

        Destroy(gameObject, 4f); // Hủy viên đạn sau 4 giây nếu không va chạm
    }

    private void Update()
    {
        if (!_isLaunched) return;
        transform.position += _direction * _speed * Time.deltaTime; // Di chuyển viên đạn theo hướng đã định
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DebugNote.Green("Đạn trúng Player mất " + _damage + " máu!");
            Destroy(gameObject);
        }
        // Kiểm tra nếu va chạm với lớp chướng ngại vật (Sử dụng dịch Bit để so sánh lớp sau này có thể mở rộng để kiểm tra nhiều lớp khác nhau mà không cần nhiều ||)
        else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Obstacle", "Environment")) != 0)
        {
            DebugNote.Red("Đạn va chạm với môi trường và bị hủy!");
            Destroy(gameObject);
        }
    }
}
