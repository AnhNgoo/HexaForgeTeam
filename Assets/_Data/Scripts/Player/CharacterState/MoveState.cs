using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : ICharacterState
{
    private CharacterBase character;
    public MoveState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
    }

    public void Exit()
    {
        character.CharacterAnimation.ResetState();
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        character.LookAtTarget();

        if (character.CharacterInput.MoveInput == Vector2.zero)
        {
            character.StateController.ChangeState(new IdleState(character));
            character.CharacterMovement.Stop();
            return;
        }

        if (character.CharacterInput.HealthRecovery && character.CharacterRecovery.RecoveryBottle > 0 && !character.CharacterInput.IsHealthRecovering && !character.IsHealthRecoveryInterrupted)
        {
            character.StateController.ChangeState(new HealthRecoveryState(character));
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

        if (!character.CharacterLockTarget.IsLockingTarget)
        {
            character.MoveNormal();
            return;
        }

        if (character.CharacterLockTarget.IsLockingTarget)
        {
            character.MoveLockTarget();
            return;
        }
    }
}
