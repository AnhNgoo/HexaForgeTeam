using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class HealthRecoveryState : ICharacterState
{
    private float healthRecoveryCompleteThreshold = 0.9f; // Ngưỡng để xác định khi nào hồi máu hoàn thành, có thể điều chỉnh tùy theo thời gian của animation
    private float recoveryDuration = 0.5f;
    private int healthRecoveryIndex;
    private GameObject recoveryBottle;
    private GameObject healingEffect;
    private CharacterBase character;
    public HealthRecoveryState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        character.CharacterInput.IsHealthRecovering = true;
        healthRecoveryIndex = character.CharacterAnimation.GetAnimationLayerWeight("Layer_1");
        character.CharacterAnimation.CrossFade("HealthRecovery", 0.1f, healthRecoveryIndex);
        StartHealthRecovery();

    }

    public void Exit()
    {
        character.CharacterMovement.Stop();
        character.CharacterAnimation.ResetState();
        character.CharacterInput.IsHealthRecovering = false;
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
        if (recoveryBottle != null)
            ObjectPooling.Instance.ReturnToPool(PoolType.RecoveryBottle, recoveryBottle);
        recoveryBottle = ObjectPooling.Instance.SpawnFromPool(PoolType.RecoveryBottle, character.HandRight.transform.position, character.HandRight.transform.rotation, character.HandRight.transform);

        if (healingEffect != null)
            ObjectPooling.Instance.ReturnToPool(PoolType.HealingEffect, healingEffect);
        healingEffect = ObjectPooling.Instance.SpawnFromPool(PoolType.HealingEffect, character.bottomEffectPoint.transform.position, Quaternion.identity, character.bottomEffectPoint.transform);
        character.CharacterWeapon.StoreWeapon();

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("HealthRecovery", healthRecoveryIndex) >= recoveryDuration);
        character.CharacterRecovery.UseRecoveryBottle();
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("HealthRecovery", healthRecoveryIndex) >= healthRecoveryCompleteThreshold);

        ObjectPooling.Instance.ReturnToPool(PoolType.RecoveryBottle, recoveryBottle);
        ObjectPooling.Instance.ReturnToPool(PoolType.HealingEffect, healingEffect);
        character.CharacterWeapon.RetrieveWeapon();

        await UniTask.Delay(500); // Delay 0.5 giây để đảm bảo uống máu đã hoàn tất trước khi chuyển trạng thái
        character.StateController.ChangeState(new IdleState(character));
    }
}
