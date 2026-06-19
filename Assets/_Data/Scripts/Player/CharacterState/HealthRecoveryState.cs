using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class HealthRecoveryState : ICharacterState
{
    private float healthRecoveryCompleteThreshold = 0.8f; // Ngưỡng để xác định khi nào hồi máu hoàn thành, có thể điều chỉnh tùy theo thời gian của animation
    private float recoveryDuration = 0.5f;
    private int healthRecoveryIndex;
    private CharacterBase character;
    public HealthRecoveryState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        healthRecoveryIndex = character.CharacterAnimation.GetAnimationLayerWeight("Health Recovery");
        character.CharacterAnimation.CrossFade("HealthRecovery", 0.1f, healthRecoveryIndex);
        StartHealthRecovery();
    }

    public void Exit()
    {
        character.CharacterMovement.Stop();
        character.CharacterAnimation.ResetState();
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        //Animation idle khi uống hồi máu
        if (character.CharacterInput.MoveInput == Vector2.zero)
        {
            character.CharacterAnimation.CrossFadeOneshot("Idle", 0.1f);
            character.CharacterMovement.Stop();
            return;
        }

        //Animation di chuyển khi hồi máu
        Move();
    }

    private void Move()
    {
        float speed = character.CharacterData.stats.speed;

        Vector3 rotationDirection = new Vector3(character.CharacterMovement.MoveDirection.x,
                                                0f,
                                                character.CharacterMovement.MoveDirection.y);

        character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
        character.CharacterAnimation.CrossFadeOneshot("Walk", 0.1f);
        character.CharacterRotate.Rotate(rotationDirection);
    }

    private async void StartHealthRecovery()
    {
        bool isHealthRecovered = false; // Đảm bảo chỉ hồi máu một lần trong quá trình animation
        GameObject recoveryBottle = ObjectPooling.Instance.SpawnFromPool(PoolType.RecoveryBottle, character.HandRight.transform.position, character.HandRight.transform.rotation, character.HandRight.transform);
        GameObject healingEffect = ObjectPooling.Instance.SpawnFromPool(PoolType.HealingEffect, character.bottomEffectPoint.transform.position, Quaternion.identity, character.bottomEffectPoint.transform);
        character.CharacterWeapon.StoreWeapon();

        while (character.CharacterAnimation.GetAnimationTime("HealthRecovery", healthRecoveryIndex) <= healthRecoveryCompleteThreshold)
        {
            if (character.CharacterAnimation.GetAnimationTime("HealthRecovery", healthRecoveryIndex) >= recoveryDuration && !isHealthRecovered)
            {
                isHealthRecovered = true;
                character.CharacterRecovery.UseRecoveryBottle();
            }
            await UniTask.Yield();
        }

        ObjectPooling.Instance.ReturnToPool(PoolType.RecoveryBottle, recoveryBottle);
        ObjectPooling.Instance.ReturnToPool(PoolType.HealingEffect, healingEffect);
        character.CharacterWeapon.RetrieveWeapon();
        character.StateController.ChangeState(new IdleState(character));
    }
}
