using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ChangeWeaponState : ICharacterState
{
    private float timeTriggerChangeWeapon = 0.3f;
    private float changeWeaponCompleteThreshold = 0.9f; // Ngưỡng để xác định khi nào thay đổi vũ khí hoàn thành, có thể điều chỉnh tùy theo thời gian của animation
    private int changeWeaponIndex;
    private CharacterBase character;
    public ChangeWeaponState(CharacterBase character)
    {
        this.character = character;
    }

    public void Enter()
    {
        character.CharacterInput.IsChangingWeapon = true;
        changeWeaponIndex = character.CharacterAnimation.GetAnimationLayerWeight("Layer_1");
        character.CharacterAnimation.CrossFade("ChangeWeapon", 0.1f, changeWeaponIndex);
        StartChangeWeapon();
    }

    public void Exit()
    {
        character.CharacterInput.IsChangingWeapon = false;
        character.CharacterAnimation.ResetState();
        character.CharacterCombat.ResetCombo();
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
        character.MoveNormal();
    }


    private async void StartChangeWeapon()
    {
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("ChangeWeapon", changeWeaponIndex) >= timeTriggerChangeWeapon);

        int nextWeaponIndex = WeaponInventorySystem.Instance.CurrentWeaponIndex + 1;
        while (nextWeaponIndex < WeaponInventorySystem.Instance.WeaponSlots.Count && WeaponInventorySystem.Instance.GetWeaponAtIndex(nextWeaponIndex) == null)
        {
            nextWeaponIndex++; // Tìm vũ khí tiếp theo không null
        }
        if (nextWeaponIndex >= WeaponInventorySystem.Instance.GetWeaponCount())
        {
            nextWeaponIndex = -1; // Quay lại vũ khí đầu tiên nếu vượt quá danh sách
        }
        WeaponInventorySystem.Instance.ChangeWeapon(nextWeaponIndex);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("ChangeWeapon", changeWeaponIndex) >= changeWeaponCompleteThreshold);

        await UniTask.Delay(100); // // Delay 0.5 giây để đảm bảo uống máu đã hoàn tất trước khi chuyển trạng thái
        character.StateController.ChangeState(new IdleState(character));
    }
}
