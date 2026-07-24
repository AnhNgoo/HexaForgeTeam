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
        if (characterBase == null)
            return;

        if (currentMP < maxMP)
        {
            currentMP += characterBase.CharacterStat.Stats.mpRegen * Time.deltaTime;
            if (currentMP > maxMP)
                currentMP = maxMP;

            mpData.MaxMP = maxMP;
            mpData.CurrentMP = currentMP;
            mpData.fullRegen = false;
            EventManager.Notify(GameEvent.OnUpdateMP, mpData);
        }
    }

    public void SetMaxMP(float maxMP, bool fullRegen = true)
    {
        this.maxMP = maxMP;
        if (currentMP > maxMP)
            currentMP = maxMP;
        if (fullRegen)
            currentMP = maxMP;

        mpData.MaxMP = maxMP;
        mpData.CurrentMP = currentMP;
        mpData.fullRegen = fullRegen;
        EventManager.Notify(GameEvent.OnUpdateMaxMP, mpData);
    }

    public void SubtractMP(float amount)
    {
        currentMP -= amount;
        if (currentMP < 0)
            currentMP = 0;

        mpData.MaxMP = maxMP;
        mpData.CurrentMP = currentMP;
        mpData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateMP, mpData);
    }

    public void AddMP(float amount)
    {
        currentMP += amount;
        if (currentMP > maxMP)
            currentMP = maxMP;

        mpData.MaxMP = maxMP;
        mpData.CurrentMP = currentMP;
        mpData.fullRegen = false;
        EventManager.Notify(GameEvent.OnUpdateMP, mpData);
    }

    public bool HasEnoughMP(float amount)
    {
        return currentMP >= amount;
    }
}
