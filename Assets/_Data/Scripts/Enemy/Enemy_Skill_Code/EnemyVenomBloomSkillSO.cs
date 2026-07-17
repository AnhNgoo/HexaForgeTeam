using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyVenomBloomSkill",
    menuName = "Enemy/Skills/Venom Bloom")]
public class EnemyVenomBloomSkillSO : EnemyAttackSkillSO
{
    [SerializeField]
    private EnemyEarthPillarsSkillSO toxicPillarsSkill;

    [SerializeField]
    private EnemyPoisonPoolSkillSO poisonPoolSkill;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context?.Enemy == null || context.Target == null)
            return;

        toxicPillarsSkill?
            .CastCataclysmAsync(context)
            .Forget();

        poisonPoolSkill?.CastBloom(context);
    }
}