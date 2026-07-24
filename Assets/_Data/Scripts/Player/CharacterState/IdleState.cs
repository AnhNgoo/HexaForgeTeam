using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : ICharacterState
{
    private CharacterBase character;
    public IdleState(CharacterBase character)
    {
        this.character = character;
    }

    public void Enter()
    {
        character.CharacterAnimation.CrossFade("Idle", 0.1f);
        character.CharacterMovement.Stop();
    }

    public void Exit()
    {
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        Debug.Log("IdleState");
        character.LookAtTarget();
        if (character.CharacterInput.MoveInput != Vector2.zero)
        {
            character.StateController.ChangeState(new MoveState(character));
            return;
        }

        if (character.CharacterInput.Skill_1)
        {
            character.Skill_1();
            return;
        }

        if (character.CharacterInput.Skill_2)
        {
            character.Skill_2();
            return;
        }
        if (character.CharacterInput.Attack)
        {
            character.Attack();
            return;
        }

        if (character.CharacterInput.HealthRecovery && character.CharacterRecovery.RecoveryBottle > 0 && !character.CharacterInput.IsHealthRecovering)
        {
            character.StateController.ChangeState(new HealthRecoveryState(character));
            return;
        }

        if (character.CharacterInput.Dodge)
        {
            character.Dodge();
            return;
        }
        if (character.CharacterInput.Jump &&
        character.CharacterMovement.IsGrounded &&
        character.CharacterStamina.HasEnoughStamina(character.CharacterData.staminaCost.jumpCost))
        {
            character.CharacterStamina.SubtractStamina(character.CharacterData.staminaCost.jumpCost);
            character.StateController.ChangeState(new JumpState(character));
            return;
        }

        if (!character.CharacterMovement.IsGrounded && character.CharacterMovement.CC.velocity.y < character.CharacterMovement.FallThreshold)
        {
            character.StateController.ChangeState(new FallState(character));
            return;
        }
    }
}


