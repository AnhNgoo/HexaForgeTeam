using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CombatState : ICharacterState
{
    private CharacterBase character;
    private bool canMove;
    public CombatState(CharacterBase character, bool canMove = false)
    {
        this.character = character;
        this.canMove = canMove;
    }
    public void Enter()
    {
        bool canEnableRootMotion = !character.CheckForNearEnemy();

        if (canEnableRootMotion)
            character.CharacterAnimation.EnableRootMotion();
        character.CharacterMovement.Stop();
        character.CharacterSkill.IsUsingSkill = true;
    }

    public void Exit()
    {
        character.CharacterAnimation.DisableRootMotion();
        character.CharacterAnimation.ResetState();
        character.CharacterSkill.IsUsingSkill = false;
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        if (!canMove)
        {
            return;
        }
        character.LookAtTarget();

        if (character.CharacterInput.MoveInput == Vector2.zero)
        {
            character.CharacterAnimation.CrossFadeOneshot("Idle", 0.1f);
            character.CharacterMovement.Stop();
            return;
        }

        if (!character.CharacterLockTarget.IsLockingTarget)
        {
            MoveNormal();
            return;
        }

        if (character.CharacterLockTarget.IsLockingTarget)
        {
            MoveLockTarget();
            return;
        }
    }

    public virtual void MoveNormal()
    {
        float speed = character.CharacterData.stats.speed;

        Vector3 rotationDirection = new Vector3(character.CharacterMovement.MoveDirection.x,
                                                0f,
                                                character.CharacterMovement.MoveDirection.y);

        character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
        character.CharacterAnimation.CrossFadeOneshot("Walk", 0.1f);
        character.CharacterRotate.Rotate(rotationDirection);
    }

    public virtual void MoveLockTarget()
    {
        float x = character.CharacterInput.MoveInput.x; // Hướng đi ngang
        float y = character.CharacterInput.MoveInput.y; // Hướng đi dọc
        float yAbs = Mathf.Abs(y); // Ngưỡng y để xác định di chuyển chéo hay thẳng
        float speed = character.CharacterData.stats.speed;

        if (x < 0 && yAbs < character.CharacterMovement.StrafeThreshold)
        {
            character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run_Strafe_Left", 0.1f);
            return;
        }

        if (x > 0 && yAbs < character.CharacterMovement.StrafeThreshold)
        {
            character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run_Strafe_Right", 0.1f);
            return;
        }
        if (y < 0)
        {
            character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run_Backward", 0.1f);
            return;
        }

        if (y > 0)
        {
            character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run", 0.1f);
            return;
        }
    }
}
