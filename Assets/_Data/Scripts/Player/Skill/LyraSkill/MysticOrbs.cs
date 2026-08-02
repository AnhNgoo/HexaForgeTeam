using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class MysticOrbs : CharacterSkillBase
{
    private int indexEnemyToAttack = 0;
    private float radiusDetectionArea = 15f; // Bán kính của khu vực phát hiện kẻ địch
    private LyraSkill_2_DetectionAreaEffect detectionAreaEffect;
    private bool isAttacking = false;
    private CancellationTokenSource delayCts;
    public MysticOrbs(CharacterBase character, CharacterSkillData skillData) : base(character, skillData)
    {
    }

    protected override async void ExecuteSkill()
    {
        if (character is not Lyra lyra)
        {
            Debug.LogError("MysticOrbs skill chỉ có thể được sử dụng bởi Lyra");
            return;
        }

        character.CanBeAttacked = false;
        EventManager.Notify(GameEvent.OnUpdateCooldownSkill2, skillData.cooldown);

        character.CharacterAnimation.CrossFade("Skill_2_1", 0.1f);
        character.CharacterWeapon.StoreWeapon();
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_2_1") > 0.3f);

        ObjectPooling.Instance?.SpawnFromPool(lyra.lyraAuraSkill_2_1,
                                            lyra.middleEffectPoint.transform.position,
                                            lyra.middleEffectPoint.transform.rotation);

        //NOTE - Hiệu ứng khu vực phát hiện kẻ địch, có thể dùng để phát hiện kẻ địch trong phạm vi
        GameObject lyraSkill_2_DetectionAreaEffect = ObjectPooling.Instance?.SpawnFromPool(lyra.lyraSkill_2_DetectionAreaEffect,
                                             lyra.middleEffectPoint.transform.position,
                                             lyra.middleEffectPoint.transform.rotation);
        if (lyraSkill_2_DetectionAreaEffect != null)
        {
            detectionAreaEffect = lyraSkill_2_DetectionAreaEffect.GetComponent<LyraSkill_2_DetectionAreaEffect>();
            if (detectionAreaEffect != null)
            {
                detectionAreaEffect.SetRadiusDetectionArea(radiusDetectionArea);
            }
        }

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_2_1") > 0.8f);

        character.CharacterAnimation.CrossFade("Skill_2_2", 0.1f);
        character.CharacterMovement.UseGravity = false;
        character.GhostEffect?.SetGhostEffect(true);

        ObjectPooling.Instance?.SpawnFromPool(lyra.lyraAuraSkill_2_2,
                                            lyra.bottomEffectPoint.transform.position,
                                            lyra.bottomEffectPoint.transform.rotation);
        await Hovering();

        // Bắt đầu tấn công quái
        isAttacking = true;
        RotateAround();
        SpawnProjectiles();
        character.CanBeAttacked = true;
        GameObject lyraAuraSkill_2_3 = ObjectPooling.Instance?.SpawnFromPool(lyra.lyraAuraSkill_2_3,
                                              lyra.middleEffectPoint.transform.position,
                                              lyra.middleEffectPoint.transform.rotation,
                                              lyra.middleEffectPoint.transform);

        delayCts?.Cancel();
        delayCts?.Dispose();
        delayCts = new CancellationTokenSource();

        await UniTask.Delay(7000, cancellationToken: delayCts.Token).SuppressCancellationThrow();

        delayCts?.Dispose();
        delayCts = null;

        ObjectPooling.Instance?.SpawnFromPool(lyra.lyraAuraSkill_2_1,
                                                 lyra.middleEffectPoint.transform.position,
                                                 lyra.middleEffectPoint.transform.rotation);

        isAttacking = false;
        character.GhostEffect?.SetGhostEffect(false);
        character.CharacterAnimation.CrossFade("Skill_2_3", 0.1f);

        if (lyraAuraSkill_2_3 != null)
            ObjectPooling.Instance.ReturnToPool(lyra.lyraAuraSkill_2_3, lyraAuraSkill_2_3);
        if (lyraSkill_2_DetectionAreaEffect != null)
            ObjectPooling.Instance.ReturnToPool(lyra.lyraSkill_2_DetectionAreaEffect, lyraSkill_2_DetectionAreaEffect);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_2_3") > 0.4f);

        character.CharacterMovement.UseGravity = true;
        character.StateController.ChangeState(new IdleState(character));
        character.CharacterWeapon.RetrieveWeapon();
    }

    public void CancelSkillDelay()
    {
        delayCts?.Cancel();
    }

    private async UniTask Hovering()
    {
        float speed = 10f; // Tốc độ bay lên
        float hoverDuration = 0.5f; // Bay lên trong t giây và dừng ở đó
        float elapsedTime = 0f;

        while (elapsedTime < hoverDuration)
        {
            character.CharacterMovement.Movement(new Vector3(0, 1, 0), speed);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        character.CharacterMovement.Stop();
    }

    private async void RotateAround()
    {
        float rotationSpeed = 360f; // Tốc độ xoay

        while (isAttacking)
        {
            character.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            await UniTask.Yield();
        }
    }

    private async void SpawnProjectiles()
    {
        float spawnInterval = 0.5f; // Thời gian giữa các lần spawn
        int waitCount = 0; // Số lần chờ

        while (isAttacking)
        {

            if (detectionAreaEffect.Enemies.Count == 0) // Nếu không không có enemy nào trong khu vực phát hiện, thì chờ và tiếp tục kiểm tra
            {
                waitCount++;
                await UniTask.Delay((int)(spawnInterval * 1000));
                if (waitCount >= 3) // Nếu chờ 3 lần mà vẫn không có enemy nào trong khu vực phát hiện, thì thoát khỏi vòng lặp
                {
                    CancelSkillDelay();
                    break;
                }
                continue;
            }

            // Nếu có enemy trong khu vực phát hiện, thì spawn projectile vào enemy đó
            if (indexEnemyToAttack < detectionAreaEffect.Enemies.Count) // Index trong phạm vi, bắn rồi tăng thêm
            {
                Transform targetEnemy = detectionAreaEffect.Enemies[indexEnemyToAttack];
                if (targetEnemy != null)
                {
                    SpawnProjectile(targetEnemy);
                }
                indexEnemyToAttack++;
            }
            else if (indexEnemyToAttack >= detectionAreaEffect.Enemies.Count) // Index vượt quá phạm vi, reset index về 0 để quay lại từ đầu
            {
                indexEnemyToAttack = 0; // Reset index để quay lại từ đầu

                Transform targetEnemy = detectionAreaEffect.Enemies[indexEnemyToAttack];
                if (targetEnemy != null)
                {
                    SpawnProjectile(targetEnemy);
                }
            }
            await UniTask.Delay((int)(spawnInterval * 1000));
        }
    }

    private void SpawnProjectile(Transform targetEnemy)
    {
        if (character is not Lyra lyra)
        {
            Debug.LogError("MysticOrbs skill chỉ có thể được sử dụng bởi Lyra");
            return;
        }

        LyraSkill_2_Projectile projectile = ObjectPooling.Instance?.SpawnFromPool(lyra.lyraSkill_2_Projectile,
                                                             character.transform.position, Quaternion.identity)?.
                                                                GetComponent<LyraSkill_2_Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(character, targetEnemy, PoolType.LyraSkill_2_HitEffect);
            projectile.OnEnemyDied += detectionAreaEffect.ClearEnemy; // Đăng ký sự kiện để loại bỏ kẻ địch khỏi danh sách khi nó chết
        }
    }
}
