using System.Collections;
using UnityEngine;

public class CharacterMelee : CharacterBase
{
    protected Coroutine trailSlashEffectCoroutine;

    protected override void Init(CharacterData data)
    {
        base.Init(data);
        characterCombat?.Init(this, InitAttackCombos());
    }

    // Override để khởi tạo các đòn tấn công riêng cho Kael
    protected override IAttackStep[] InitAttackCombos()
    {
        return new IAttackStep[4]
        {
            new AttackMeleeStep_1(),
            new AttackMeleeStep_2(),
            new AttackMeleeStep_3(),
            new AttackMeleeStep_4()
        };
    }

    public virtual void PlaySlashEffect(int index)
    {
        EventManager.Instance?.Notify(GameEvent.OnPlaySlashEffect, index);
    }

    public virtual void PlayTrailSlashEffect()
    {
        if (trailSlashEffectCoroutine != null)
        {
            StopCoroutine(trailSlashEffectCoroutine);
        }
        trailSlashEffectCoroutine = StartCoroutine(PlayTrailSlashEffectCoroutine());
    }
    protected virtual IEnumerator PlayTrailSlashEffectCoroutine()
    {
        EventManager.Instance?.Notify(GameEvent.OnEnableTrailSlashEffect);
        float delay = characterCombat != null ? characterCombat.NextAttackTime : 0.7f;
        yield return new WaitForSeconds(delay);
        EventManager.Instance?.Notify(GameEvent.OnDisableTrailSlashEffect);
    }
}
