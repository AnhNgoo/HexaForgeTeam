using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGoldFalling : MonoBehaviour
{
    private CharacterBase characterBase;
    public void Init(CharacterBase characterBase)
    {
        this.characterBase = characterBase;
    }
    // Lấy vàng khi chết = vàng hiện tại + vàng cần có lúc lên lv hiện tại
    private int GetGoldOnDeath()
    {
        return GoldManager.Instance.CurrentGold + characterBase.CharacterLevel.StatGainedLevelUp.GetLevelUpCost(characterBase.CharacterLevel.CurrentLevel);
    }

    // Tạo vàng rơi và init số vàng rơi ra
    public void CreateGoldFalling()
    {
        int goldOnDeath = GetGoldOnDeath();
        if (goldOnDeath > 0)
        {
            ObjectPooling.Instance.SpawnFromPool(PoolType.GoldFalling, transform.position, Quaternion.identity)?.
                                    GetComponent<PickUpGoldFalling>().
                                    Init(goldOnDeath);
        }
    }
}
