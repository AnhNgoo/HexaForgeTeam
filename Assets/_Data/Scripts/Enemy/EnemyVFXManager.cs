using UnityEngine;

public class EnemyVFXManager : MonoBehaviour
{
    private EnemyBase _enemyBase;

    [Header("VFX Transform Anchors")]
    [SerializeField] private Transform _chestAnchor; // Điểm neo để gắn hiệu ứng khi bị đánh trúng, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi Enemy bị đánh trúng
    [SerializeField] private Transform _headAnchor; // Điểm neo để gắn hiệu ứng khi bị đánh trúng, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi Enemy bị đánh trúng
    [SerializeField] private Transform _footAnchor; // Điểm neo để gắn hiệu ứng khi bị đánh trúng, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi Enemy bị đánh trúng
    [Header("VFX Settings")]
    [SerializeField] private float _staggerHitReactDuration = 0.5f; // Thời gian hiển thị hiệu ứng phản ứng khi bị đánh trúng trong trạng thái stagger, có thể được thiết lập trong Inspector để điều chỉnh thời gian hiển thị hiệu ứng phản ứng khi bị đánh trúng trong trạng thái stagger
    [SerializeField] private PoolType _staggerVFX = PoolType.None; // Loại VFX để sử dụng khi bị đánh trúng trong trạng thái stagger, có thể được thiết lập trong Inspector để chọn loại VFX phù hợp với hiệu ứng phản ứng khi bị đánh trúng trong trạng thái stagger
    [SerializeField] private Vector3 _staggerVFXOffset = new Vector3(0f, 0.5f, 0f); // Offset để điều chỉnh vị trí spawn của VFX khi bị đánh trúng trong trạng thái stagger, có thể được thiết lập trong Inspector để điều chỉnh vị trí spawn của VFX khi Enemy bị đánh trúng trong trạng thái stagger
    [SerializeField] private Vector3 _staggerVFXEuler; // Euler angles để điều chỉnh rotation của VFX khi bị đánh trúng trong trạng thái stagger, có thể được thiết lập trong Inspector để điều chỉnh rotation của VFX khi Enemy bị đánh trúng trong trạng thái stagger
    [SerializeField] private float _staggerVFXScale = 1f; // Scale để điều chỉnh kích thước của VFX khi bị đánh trúng trong trạng thái stagger, có thể được thiết lập trong Inspector để điều chỉnh kích thước của VFX khi Enemy bị đánh trúng trong trạng thái stagger
    private GameObject _staggerVFXInstance; // Biến để lưu reference đến instance của VFX khi bị đánh trúng trong trạng thái stagger, có thể dùng để quản lý việc dừng và hủy VFX khi cần thiết (ví dụ như khi thoát khỏi trạng thái stagger hoặc khi bị đánh trúng lại trong khi đang stagger)

    public float StaggerHitReactDuration => _staggerHitReactDuration;

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase; // Lưu reference đến EnemyBase để sử dụng sau này, có thể dùng để truy cập các thành phần khác của Enemy khi cần thiết

        if (_chestAnchor == null) _chestAnchor = transform; // Nếu không có chestAnchor được thiết lập thì sử dụng transform của Enemy làm điểm neo mặc định để đảm bảo rằng hiệu ứng sẽ được gắn vào Enemy một cách chính xác khi bị đánh trúng
        if (_headAnchor == null) _headAnchor = transform; // Nếu không có headAnchor được thiết lập thì sử dụng transform của Enemy làm điểm neo mặc định để đảm bảo rằng hiệu ứng sẽ được gắn vào Enemy một cách chính xác khi bị đánh trúng
        if (_footAnchor == null) _footAnchor = transform; // Nếu không có foot

        Debug.Log($"{gameObject.name} - EnemyVFXManager đã được khởi tạo!"); // Log để kiểm tra xem EnemyVFXManager đã được khởi tạo hay chưa, có thể giúp phát hiện lỗi trong quá trình phát triển
    }

    /// <summary> 
    /// Gọi kích hoạt hiệu ứng từ pool tại một điểm neo cụ thể của quái vật
    /// </summary>
    public void PlayVFX(PoolType vfxType, Vector3 offset = default)
    {
        if (vfxType == PoolType.None) return; // Nếu loại VFX là None thì không làm gì để tránh lỗi và đảm bảo rằng chỉ những loại VFX hợp lệ mới được kích hoạt

        Transform targetAnchor = GetAnchorByVFXType(vfxType); // Lấy điểm neo tương ứng với loại VFX để gắn hiệu ứng một cách chính xác
        Vector3 spawnPosition = targetAnchor.position + offset; // Tính toán vị trí spawn của VFX dựa trên điểm neo và offset để đảm bảo rằng hiệu ứng sẽ được gắn vào Enemy một cách chính xác khi bị đánh trúng

        ObjectPooling.Instance.SpawnFromPool(vfxType, spawnPosition, targetAnchor.rotation); // Spawn VFX từ pool tại vị trí đã tính toán với rotation mặc định để đảm bảo rằng hiệu ứng sẽ được hiển thị đúng cách khi Enemy bị đánh trúng
    }

    public void PlayStaggerVFX()
    {
        if (_enemyBase == null || _enemyBase.Data == null) return;
        if (_staggerVFX == PoolType.None) return;

        StopStaggerVFX();

        Transform anchor = _headAnchor != null ? _headAnchor : transform;

        Vector3 position = anchor.position + anchor.TransformDirection(_staggerVFXOffset);
        Quaternion rotation = anchor.rotation * Quaternion.Euler(_staggerVFXEuler);

        _staggerVFXInstance = ObjectPooling.Instance.SpawnFromPool(_staggerVFX, position, rotation);

        if (_staggerVFXInstance == null) return;

        _staggerVFXInstance.transform.SetParent(anchor);
        _staggerVFXInstance.transform.localPosition = _staggerVFXOffset;
        _staggerVFXInstance.transform.localRotation = Quaternion.Euler(_staggerVFXEuler);
        _staggerVFXInstance.transform.localScale = Vector3.one * _staggerVFXScale;
    }

    public void StopStaggerVFX()
    {
        if (_staggerVFXInstance == null) return;

        _staggerVFXInstance.transform.SetParent(null);
        if (_staggerVFX != PoolType.None)
        {
            ObjectPooling.Instance.ReturnToPool(_staggerVFX, _staggerVFXInstance);
        }
        else
        {
            _staggerVFXInstance.SetActive(false);
        }
        _staggerVFXInstance = null;
    }

    private Transform GetAnchorByVFXType(PoolType vfxType)
    {
        // Lấy điểm neo tương ứng với loại VFX để gắn hiệu ứng một cách chính xác, có thể mở rộng sau này để có nhiều loại VFX khác nhau và điểm neo tương ứng
        switch (vfxType)
        {
            case PoolType.None:
                return _chestAnchor;
            // case PoolType.SuspicionVFX:
            //     return _headAnchor;
            // case PoolType.None:
            //     return _footAnchor;
            default:
                Debug.LogWarning($"Không tìm thấy điểm neo phù hợp cho loại VFX {vfxType}! Sử dụng transform của Enemy làm điểm neo mặc định."); // Cảnh báo nếu không tìm thấy điểm neo phù hợp để giúp phát hiện lỗi trong quá trình phát triển
                return transform; // Trả về transform của Enemy làm điểm neo mặc định nếu không tìm thấy điểm neo phù hợp để đảm bảo rằng hiệu ứng sẽ được gắn vào Enemy một cách chính xác khi bị đánh trúng
        }
    }
}
