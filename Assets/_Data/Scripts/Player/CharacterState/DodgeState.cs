using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DodgeState : ICharacterState
{
    private CharacterBase character;
    public DodgeState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        character.CharacterMovement.IsDodging = true;
        Dodge();
    }


    public void Exit()
    {
        character.CharacterMovement.IsDodging = false;
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
        float timer = 0f;
        while (timer < character.CharacterMovement.DodgeDuration)
        {
            character.CharacterMovement.Dodge(dodgeDirection, character.CharacterData.stats.speed);
            character.CharacterRotate.Rotate(new Vector3(dodgeDirection.x, 0f, dodgeDirection.y));
            timer += Time.deltaTime;
            await UniTask.Yield();
        }
        character.StateController.ChangeState(new IdleState(character));
    }
}
