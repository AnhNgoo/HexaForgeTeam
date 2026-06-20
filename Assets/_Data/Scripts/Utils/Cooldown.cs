using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Cooldown
{
    private float timer = 0f;
    public bool IsOnCooldown => timer > 0f;

    public Cooldown()
    {
        timer = 0f;
    }

    /// <summary>
    /// Bắt đầu cooldown với thời gian cooldown được chỉ định. Trong khi cooldown đang hoạt động, IsOnCooldown sẽ trả về true.
    /// </summary>
    /// <param name="cooldown"></param>
    public void StartCooldown(float cooldown)
    {
        timer = cooldown;
        CooldownTimer();
    }

    public float GetRemainingCooldown()
    {
        return timer;
    }
    /// <summary>
    /// Hàm này sẽ giảm timer theo thời gian thực. Khi timer giảm về 0, cooldown kết thúc và IsOnCooldown sẽ trả về false. Hàm này sử dụng UniTask để chạy song song với các tác
    /// </summary>
    private async void CooldownTimer()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            await UniTask.Yield();
        }
        timer = 0f;
    }
}
