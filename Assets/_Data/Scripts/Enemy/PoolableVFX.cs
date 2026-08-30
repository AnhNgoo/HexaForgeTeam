using UnityEngine;
using UnityEngine.Playables;

public class PoolableVFX : MonoBehaviour, IPoolable
{
    [Header("Pool Setup")]
    [SerializeField] private PoolType _vfxPoolType;

    private ParticleSystem[] _particleSystems;
    private PlayableDirector _playableDirector;

    private bool _isTracking;
    private bool _hasPlayableDirector;

    public PoolType PoolType => _vfxPoolType;

    private void Awake()
    {
        // Lấy tất cả Particle System kể cả object con
        _particleSystems =
            GetComponentsInChildren<ParticleSystem>(true);

        // Tìm Playable Director trong prefab
        _playableDirector =
            GetComponentInChildren<PlayableDirector>(true);

        _hasPlayableDirector =
            _playableDirector != null;
    }

    public void OnSpawnFromPool()
    {
        _isTracking = false;

        // =========================
        // RESET PLAYABLE
        // =========================

        if (_playableDirector != null)
        {
            _playableDirector.Stop();

            _playableDirector.time = 0;

            _playableDirector.Evaluate();
        }

        // =========================
        // RESET PARTICLES
        // =========================

        if (_particleSystems != null)
        {
            foreach (ParticleSystem ps in _particleSystems)
            {
                if (ps == null)
                    continue;

                ps.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

                ps.Clear();
            }
        }

        // =========================
        // PLAY
        // =========================

        if (_playableDirector != null)
        {
            _playableDirector.Play();
        }

        if (_particleSystems != null)
        {
            foreach (ParticleSystem ps in _particleSystems)
            {
                if (ps == null)
                    continue;

                ps.Play(true);
            }
        }

        _isTracking = true;
    }

    private void Update()
    {
        if (!_isTracking)
            return;

        // ==========================================
        // PLAYABLE VFX
        // ==========================================

        if (_hasPlayableDirector)
        {
            if (_playableDirector == null)
                return;

            // Timeline đã chạy hết
            if (_playableDirector.state != PlayState.Playing)
            {
                _isTracking = false;

                ReturnToPool();
            }

            return;
        }

        // ==========================================
        // PARTICLE ONLY VFX
        // ==========================================

        if (_particleSystems == null ||
            _particleSystems.Length == 0)
        {
            _isTracking = false;

            ReturnToPool();

            return;
        }

        bool anyAlive = false;

        foreach (ParticleSystem ps in _particleSystems)
        {
            if (ps != null && ps.IsAlive(true))
            {
                anyAlive = true;
                break;
            }
        }

        if (!anyAlive)
        {
            _isTracking = false;

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (ObjectPooling.Instance == null)
            return;

        if (_vfxPoolType == PoolType.None)
            return;

        ObjectPooling.Instance.ReturnToPool(
            _vfxPoolType,
            gameObject
        );
    }

    public void OnReturnToPool()
    {
        _isTracking = false;

        // =========================
        // STOP PLAYABLE
        // =========================

        if (_playableDirector != null)
        {
            _playableDirector.Stop();

            _playableDirector.time = 0;

            _playableDirector.Evaluate();
        }

        // =========================
        // STOP PARTICLES
        // =========================

        if (_particleSystems != null)
        {
            foreach (ParticleSystem ps in _particleSystems)
            {
                if (ps == null)
                    continue;

                ps.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

                ps.Clear();
            }
        }
    }
}