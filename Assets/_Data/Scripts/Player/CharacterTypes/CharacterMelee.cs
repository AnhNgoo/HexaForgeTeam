using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CharacterMelee : CharacterBase
{
    [Header("Melee Attack Effects")]
    public GameObject meleeAttackEffectPoint_1;
    public PoolType meleeAttackEffect_1 = PoolType.SlashEffect_1;
    public GameObject meleeAttackEffectPoint_2;
    public PoolType meleeAttackEffect_2 = PoolType.SlashEffect_1;
    public GameObject meleeAttackEffectPoint_3;
    public PoolType meleeAttackEffect_3 = PoolType.SlashEffect_1;
    public GameObject meleeAttackEffectPoint_4;
    public PoolType meleeAttackEffect_4 = PoolType.Earthquake_1;

    protected override void LoadEffectPoints()
    {
        base.LoadEffectPoints();
        if (meleeAttackEffectPoint_1 == null)
            meleeAttackEffectPoint_1 = effectPoints?.transform.Find("MeleeAttackEffectPoint_1")?.gameObject;
        if (meleeAttackEffectPoint_2 == null)
            meleeAttackEffectPoint_2 = effectPoints?.transform.Find("MeleeAttackEffectPoint_2")?.gameObject;
        if (meleeAttackEffectPoint_3 == null)
            meleeAttackEffectPoint_3 = effectPoints?.transform.Find("MeleeAttackEffectPoint_3")?.gameObject;
        if (meleeAttackEffectPoint_4 == null)
            meleeAttackEffectPoint_4 = effectPoints?.transform.Find("MeleeAttackEffectPoint_4")?.gameObject;
    }

    public override void Attack()
    {
        if (!CheckStaminaAndMPForAttack())
            return;

        if (characterCombat.CurrentComboIndex == 0) // Chỉ áp sát mục tiêu nếu đây là đòn tấn công đầu tiên trong chuỗi combo
            MeleeSnapToTarget();
        characterCombat?.TryAttack();
    }
}

