using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPData
{
    public float MaxMP;
    public float CurrentMP;
    public bool fullRegen;
}
public class CharacterMP : MonoBehaviour
{
    [SerializeField] private float maxMP = 0;
    public float MaxMP => maxMP;
    [SerializeField] private float currentMP;
    public float CurrentMP => currentMP;

    private CharacterBase characterBase;
    private MPData mpData = new MPData();

    public void Init(CharacterBase characterBase)
    {
        this.characterBase = characterBase;
    }

    private void Update()
    {
        AddMPOverTime();
    }

    public void SetMaxMP(float maxMP, bool fullRegen = true)
    {
        float normalizedMaxMP = Mathf.Max(1f, maxMP);

        this.maxMP = normalizedMaxMP;
        currentMP = Mathf.Clamp(currentMP, 0, normalizedMaxMP);
        if (fullRegen)
            currentMP = normalizedMaxMP;

        mpData.MaxMP = normalizedMaxMP;
        mpData.CurrentMP = currentMP;
        mpData.fullRegen = fullRegen;
        EventManager.Notify(GameEvent.OnUpdateMaxMP, mpData);
    }

    public void SubtractMP(float amount)
    {
        currentMP -= amount;
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);

        mpData.MaxMP = maxMP;
        mpData.CurrentMP = currentMP;
        mpData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateMP, mpData);
    }

    public void AddMP(float amount)
    {
        currentMP += amount;
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);

        mpData.MaxMP = maxMP;
        mpData.CurrentMP = currentMP;
        mpData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateMP, mpData);
    }

    public bool HasEnoughMP(float amount)
    {
        return currentMP >= amount;
    }

    public void AddMPOverTime()
    {
        if (characterBase == null)
            return;

        if (currentMP < maxMP)
        {
            currentMP += characterBase.CharacterStat.Stats.mpRegen * Time.deltaTime;
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);

            mpData.MaxMP = maxMP;
            mpData.CurrentMP = currentMP;
            EventManager.Notify(GameEvent.OnUpdateMP, mpData);
        }
    }

    public void SubtractMPOverTime(float amount)
    {
        if (characterBase == null)
            return;

        if (currentMP > 0)
        {
            currentMP -= amount * Time.deltaTime;
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);

            mpData.MaxMP = maxMP;
            mpData.CurrentMP = currentMP;
            EventManager.Notify(GameEvent.OnUpdateMP, mpData);
        }
    }
}
