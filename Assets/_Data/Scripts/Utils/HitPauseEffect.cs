using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class HitPauseEffect
{
    /// <summary>
    /// Tạm dừng thời gian trong một khoảng thời gian nhất định.
    /// </summary>
    /// <param name="pauseDuration"></param>
    /// <param name="timeScale"></param>
    public async void PlayHitPause(float pauseDuration = 0.1f, float timeScale = 0f)
    {
        Time.timeScale = timeScale; // Đặt tỷ lệ thời gian
        await UniTask.Delay((int)(pauseDuration * 1000), ignoreTimeScale: true); // Đợi một khoảng thời gian nhất định
        Time.timeScale = 1f; // Tiếp tục thời gian
    }
}
