using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttackStep_4 : IAttackStep
{
    public string AttackStateName => "Attack_4";
    private float timeShake = 0.3f;
    public void Attack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
        Vector2 lungeDirection = new Vector2(character.transform.forward.x, character.transform.forward.z).normalized;

        character.CharacterMovement.Lunge(lungeDirection);

        PlaySlashEffect();
        ShakeWhenCompleted(character);
    }

    private async void PlaySlashEffect()
    {
        await UniTask.Delay(300);
        EventManager.Instance?.Notify(GameEvent.OnSlashEffect);
    }
    private async void ShakeWhenCompleted(CharacterBase character)
    {
        await UniTask.WaitUntil(() =>
            {
                AnimatorStateInfo stateInfo = character.CharacterAnimation.Animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.IsName(AttackStateName) &&
                       stateInfo.normalizedTime >= timeShake &&
                       !character.CharacterAnimation.Animator.IsInTransition(0);
            });
        CameraShake.Instance?.Shake();
    }
}
