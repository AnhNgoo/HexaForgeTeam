using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttackStep_1 : IAttackStep
{
    public string AttackStateName => "Attack_1";

    public void Attack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade("Attack_1", 0.1f);

        Vector2 lungeDirection = new Vector2(character.transform.forward.x, character.transform.forward.z).normalized;
        character.CharacterMovement.Lunge(lungeDirection);

        PlaySlashEffect();
    }

    private async void PlaySlashEffect()
    {
        await UniTask.Delay(200);
        EventManager.Instance.Notify(GameEvent.OnSlashEffect);
    }
}
