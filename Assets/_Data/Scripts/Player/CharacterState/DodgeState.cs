using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DodgeState : ICharacterState
{
    private float shadowSpawnInterval = 0.03f;//thời gian mỗi lần tạo bóng khi đang dodge, có thể điều chỉnh tùy theo nhu cầu
    private CharacterBase character;
    public DodgeState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        Dodge();
    }


    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {

    }

    private async void Dodge()
    {
        Vector2 dodgeDirection = character.CharacterMovement.MoveDirection;

        if (dodgeDirection == Vector2.zero)//Nếu không di chuyển thì lấy hướng hiện tại của nhân vật
            dodgeDirection = new Vector2(character.transform.forward.x, character.transform.forward.z).normalized;

        character.CharacterAnimation.CrossFade("Dodge", 0.1f);
        character.CharacterMovement.Dodge(dodgeDirection, character.CharacterData.stats.speed);
        // character.dashShadowEffect.CreateShadowEffect();
        CreateDashShadowEffect(); // Tạo hiệu ứng bóng trong khi đang dodge

        while (character.CharacterMovement.IsDodging)
        {
            character.CharacterRotate.Rotate(new Vector3(dodgeDirection.x, 0f, dodgeDirection.y));
            await UniTask.Yield();
        }

        character.StateController.ChangeState(new IdleState(character));
    }

    private async void CreateDashShadowEffect()
    {
        while (character.CharacterMovement.IsDodging)
        {
            character.DashShadowEffect.CreateShadowEffect();
            await UniTask.Delay((int)(shadowSpawnInterval * 1000)); // Tạo bóng mỗi 0.1 giây, có thể điều chỉnh tùy theo nhu cầu
        }
    }
}
