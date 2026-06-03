using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PoolableVFX : MonoBehaviour, IPoolable
{
    [Header("Pool Setup")]
    [SerializeField] private PoolType _vfxPoolType; // Loại pool để xác định loại VFX này thuộc về pool nào, có thể được thiết lập trong Inspector để quản lý các loại VFX khác nhau trong hệ thống pooling

    private ParticleSystem _particleSystem; // Tham chiếu đến ParticleSystem của VFX, có thể gán trực tiếp trên editor hoặc lấy reference trong Awake để đảm bảo rằng VFX có thể tự động chơi và tắt khi hoàn thành hiệu ứng
    private bool _isTracking = false; // Biến để theo dõi trạng thái của VFX, có thể dùng để kiểm soát việc tắt VFX sau khi hoàn thành hiệu ứng để tránh lỗi và đảm bảo rằng VFX sẽ được trả về pool đúng cách
    public PoolType PoolType => _vfxPoolType;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>(); // Lấy reference đến ParticleSystem của VFX để sử dụng sau này, có thể dùng để tự động chơi và tắt VFX khi hoàn thành hiệu ứng

        var mainModule = _particleSystem.main;
        mainModule.stopAction = ParticleSystemStopAction.None; // Đặt stopAction của ParticleSystem thành None để đảm bảo rằng VFX sẽ không tự động tắt khi hoàn thành hiệu ứng, có thể dùng để kiểm soát việc tắt VFX sau khi hoàn thành hiệu ứng một cách chính xác hơn trong code
    }

    public void OnSpawnFromPool()
    {
        if (_particleSystem != null)
        {
            _particleSystem.Clear(); // Xóa các hạt còn lại từ lần chơi trước để đảm bảo rằng VFX sẽ hiển thị đúng hiệu ứng mới khi được spawn từ pool
            _particleSystem.Play(); // Tự động chơi VFX khi được spawn từ pool để đảm bảo rằng hiệu ứng sẽ được hiển thị ngay lập tức khi
        }
        _isTracking = true; // Bắt đầu theo dõi trạng thái của VFX để kiểm soát việc tắt VFX sau khi hoàn thành hiệu ứng
    }

    private void Update()
    {
        if (!_isTracking) return; // Nếu không đang theo dõi trạng thái của VFX thì không làm gì để tiết kiệm hiệu năng và tránh lỗi

        if (_particleSystem.IsAlive(true))
        {
            _isTracking = false; // Dừng theo dõi trạng thái của VFX khi hiệu ứng đã hoàn thành để tiết kiệm hiệu năng và tránh lỗi
            ReturnToPool(); // Trả VFX về pool khi hiệu ứng đã hoàn thành để đảm bảo rằng VFX sẽ được tái sử dụng đúng cách và tiết kiệm tài nguyên
        }
    }

    public void ReturnToPool()
    {
        if (ObjectPooling.Instance != null && _vfxPoolType != PoolType.None)
        {
            ObjectPooling.Instance.ReturnToPool(_vfxPoolType, gameObject); // Trả VFX về pool dựa trên loại pool đã thiết lập để đảm bảo rằng VFX sẽ được tái sử dụng đúng cách và tiết kiệm tài nguyên
        }
        else
        {
            Debug.LogWarning($"PoolableVFX {gameObject.name} không thể trả về pool vì ObjectPooling.Instance hoặc _vfxPoolType chưa được thiết lập đúng cách!"); // Cảnh báo nếu ObjectPooling.Instance hoặc _vfxPoolType chưa được thiết lập đúng cách để giúp phát hiện lỗi trong quá trình phát triển
            Destroy(gameObject); // Hủy VFX nếu không thể trả về pool để tránh lỗi và đảm bảo rằng VFX sẽ không tồn tại vô hạn trong trường hợp có lỗi trong hệ thống pooling
        }
    }

    public void OnReturnToPool()
    {
        if (_particleSystem != null)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Dừng và xóa các hạt còn lại khi VFX được trả về pool để đảm bảo rằng VFX sẽ hiển thị đúng hiệu ứng mới khi được spawn lại từ pool
        }
        _isTracking = false; // Đảm bảo rằng VFX sẽ không bị theo dõi sai trạng thái khi được trả về pool để tránh lỗi và đảm bảo rằng VFX sẽ được tái sử dụng đúng cách
    }
}
