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
        //Chuyển về FallState nếu đang ở trên không và bắt đầu rơi
        if (!character.CharacterMovement.IsGrounded && character.CharacterMovement.CC.velocity.y < character.CharacterMovement.FallThreshold)
        {
            character.StateController.ChangeState(new FallState(character));
            return;
        }

        //Animation idle khi uống hồi máu
        if (character.CharacterMovement.MoveDirection == Vector2.zero)
        {
            character.CharacterAnimation.CrossFadeOneshot("Idle", 0.1f);
            character.CharacterMovement.Stop();
            return;
        }

        //Animation di chuyển khi hồi máu
        MoveNormal();
    }

    private void MoveNormal()
    {
        float xAbs = Mathf.Abs(character.JoystickInput.x);
        float yAbs = Mathf.Abs(character.JoystickInput.y);
        float inputSpeed = Mathf.Max(xAbs, yAbs);// Giá trị lớn nhất giữa x và y của joystick để xác định tốc độ di chuyển
        float speed = character.CharacterData.stats.speed;

        Vector3 rotationDirection = new Vector3(character.CharacterMovement.MoveDirection.x,
                                                0f,
                                                character.CharacterMovement.MoveDirection.y);

        if (inputSpeed > 0)
        {
            character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Walk", 0.1f);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }
    }

    private async void StartHealthRecovery()
    {
        bool isHealthRecovered = false; // Đảm bảo chỉ hồi máu một lần trong quá trình animation
        character.IsHealthRecovering = true;

        while (character.CharacterAnimation.GetAnimationTime("HealthRecovery", healthRecoveryIndex) <= healthRecoveryCompleteThreshold)
        {
            if (character.CharacterAnimation.GetAnimationTime("HealthRecovery", healthRecoveryIndex) >= recoveryDuration && !isHealthRecovered)
            {
                isHealthRecovered = true;
                DebugNote.Green("Gọi hàm hồi máu ở đây");
            }
            await UniTask.Yield();
        }
        character.IsHealthRecovering = false;
        character.StateController.ChangeState(new IdleState(character));
    }
}
