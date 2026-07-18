using UnityEngine;

public class CharacterPoisonStatus : MonoBehaviour
{
    [SerializeField] private float exposureThreshold = 100f;
    [SerializeField] private float exposureDecayPerSecond = 12f;
    [SerializeField] private float poisonDuration = 5f;
    [SerializeField] private float poisonTickInterval = 1f;
    [SerializeField] private float poisonDamagePerTick = 8f;

    private ITakeDamage _damageable;
    private float _exposure;
    private float _poisonEndTime;
    private float _nextDamageTime;
    private GameObject _attacker;

    public float Exposure => _exposure;
    public bool IsPoisoned => Time.time < _poisonEndTime;

    private void Awake()
    {
        _damageable = GetComponent<ITakeDamage>();
    }

    private void Update()
    {
        if (!IsPoisoned)
        {
            _exposure = Mathf.MoveTowards(_exposure, 0f, exposureDecayPerSecond * Time.deltaTime);

            return;
        }

        if (Time.time < _nextDamageTime || _damageable == null)
            return;

        _nextDamageTime = Time.time + poisonTickInterval;

        _damageable.TakeDamage(new DamageInfo
        {
            damageAmount = poisonDamagePerTick,
            attacker = _attacker,

            // Tận dụng cờ sẵn có để damage theo thời gian không gây HitState.
            isFromSafeZoneEffect = true
        });
    }

    public void AddExposure(float amount, GameObject attacker)
    {
        if (amount <= 0f)
            return;

        _attacker = attacker;
        _exposure = Mathf.Min(exposureThreshold, _exposure + amount);

        if (_exposure < exposureThreshold)
            return;

        _exposure = 0f;
        _poisonEndTime = Time.time + poisonDuration;
        _nextDamageTime = Time.time;
    }

    private void OnDisable()
    {
        _exposure = 0f;
        _poisonEndTime = 0f;
        _nextDamageTime = 0f;
        _attacker = null;
    }
}