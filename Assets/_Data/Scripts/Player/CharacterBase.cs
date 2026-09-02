using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterAnimation))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterRotate))]
[RequireComponent(typeof(CharacterWeapon))]
[RequireComponent(typeof(CharacterCombat))]
[RequireComponent(typeof(CharacterLockTarget))]
[RequireComponent(typeof(CharacterSkill))]
[RequireComponent(typeof(CharacterInput))]
[RequireComponent(typeof(CharacterHealth))]
[RequireComponent(typeof(CharacterRecovery))]
[RequireComponent(typeof(SafeZoneChecker))]
[RequireComponent(typeof(CharacterMeleeHitbox))]
[RequireComponent(typeof(CharacterCinematic))]
[RequireComponent(typeof(CharacterStat))]
[RequireComponent(typeof(CharacterStamina))]
[RequireComponent(typeof(CharacterMP))]
[RequireComponent(typeof(CharacterLevel))]
[RequireComponent(typeof(CharacterGoldFalling))]
[RequireComponent(typeof(CharacterRelic))]
[RequireComponent(typeof(CharacterSound))]
public abstract class CharacterBase : LoadComponents, ITakeDamage, IPoolable
{
    [Header("Character Data")]
    [SerializeField] protected CharacterData characterData;
    public CharacterData CharacterData => characterData;
    [SerializeField] protected float minTakeDamage = 30f;
    [Header("Respawn Settings")]
    [SerializeField] protected float respawnDelay = 3f; // Thời gian delay trước khi respawn
    [Header("Dust Effect Settings")]
    [SerializeField] protected ParticleSystem dustEffect;
    protected bool isDustEffectPlaying = false;
    [Header("Check Near Enemy Settings")]
    [SerializeField] protected LayerMask enemyLayer; // Lớp của kẻ địch để kiểm tra va chạm khi kiểm tra kẻ địch gần trước mặt
    [SerializeField] protected float ZoffsetCheckForNearEnemy = 2f; // Khoảng cách Z để kiểm tra kẻ địch gần trước mặt không để tắt root motion khi tấn công
    [SerializeField] protected float radiusCheckForNearEnemy = 1.5f; // Bán kính để kiểm tra kẻ địch gần trước mặt không để tắt root motion khi tấn công
    [SerializeField] protected bool debugModeCheckForNearEnemy = false; // Bật để hiển thị gizmo kiểm tra kẻ địch gầns
    [Header("Melee Snap Threshold")]
    [SerializeField] protected Vector2 meleeSnapThreshold = new Vector2(2.5f, 15f); // Tầm áp sát tối thiểu và tối đa để kích hoạt snap

    [Header("Character Models")]
    [SerializeField] protected GameObject visuals;
    [SerializeField] protected GameObject characterVisual;
    [Tooltip("Tay phải của nhân vật, gán thủ công với gameobject của nhân vật có tên là handslot.r")]
    [SerializeField] protected GameObject handRight;
    public GameObject HandRight => handRight;

    [Header("Character Components")]
    [SerializeField] protected CharacterAnimation characterAnimation;
    public CharacterAnimation CharacterAnimation => characterAnimation;
    [SerializeField] protected CharacterMovement characterMovement;
    public CharacterMovement CharacterMovement => characterMovement;
    [SerializeField] protected CharacterRotate characterRotate;
    public CharacterRotate CharacterRotate => characterRotate;
    [SerializeField] protected CharacterWeapon characterWeapon;
    public CharacterWeapon CharacterWeapon => characterWeapon;
    [SerializeField] protected CharacterCombat characterCombat;
    public CharacterCombat CharacterCombat => characterCombat;
    [SerializeField] protected CharacterLockTarget characterLockTarget;
    public CharacterLockTarget CharacterLockTarget => characterLockTarget;
    [SerializeField] protected CharacterSkill characterSkill;
    public CharacterSkill CharacterSkill => characterSkill;
    [SerializeField] protected CharacterInput characterInput;
    public CharacterInput CharacterInput => characterInput;
    [SerializeField] protected CharacterHealth characterHealth;
    public CharacterHealth CharacterHealth => characterHealth;
    [SerializeField] protected CharacterRecovery characterRecovery;
    public CharacterRecovery CharacterRecovery => characterRecovery;
    [SerializeField] protected CharacterMeleeHitbox characterMeleeHitbox;
    public CharacterMeleeHitbox CharacterMeleeHitbox => characterMeleeHitbox;
    [SerializeField] protected CharacterCinematic characterCinematic;
    public CharacterCinematic CharacterCinematic => characterCinematic;
    [SerializeField] protected CharacterStat characterStat;
    public CharacterStat CharacterStat => characterStat;
    [SerializeField] protected CharacterStamina characterStamina;
    public CharacterStamina CharacterStamina => characterStamina;
    [SerializeField] protected CharacterMP characterMP;
    public CharacterMP CharacterMP => characterMP;
    [SerializeField] protected CharacterLevel characterLevel;
    public CharacterLevel CharacterLevel => characterLevel;
    [SerializeField] protected CharacterGoldFalling characterGoldFalling;
    public CharacterGoldFalling CharacterGoldFalling => characterGoldFalling;
    [SerializeField] protected CharacterRelic characterRelic;
    public CharacterRelic CharacterRelic => characterRelic;
    [SerializeField] protected CharacterSound characterSound;
    public CharacterSound CharacterSound => characterSound;

    [Header("Character Effect General")]
    [SerializeField] protected GameObject effectPoints;
    public GameObject middleEffectPoint;
    public GameObject bottomEffectPoint;
    public PoolType hitEffect_1 = PoolType.HitEffect_1;
    public PoolType hitEffect_2 = PoolType.HitEffect_2;
    public GameObject punchEffectPoint_1;
    public PoolType punchEffect_1 = PoolType.PunchEffect_1;
    public GameObject punchEffectPoint_2;
    public PoolType punchEffect_2 = PoolType.PunchEffect_2;
    public GameObject punchEffectPoint_3;
    public PoolType punchEffect_3 = PoolType.PunchEffect_2;
    public GameObject punchEffectPoint_4;
    public PoolType punchEffect_4 = PoolType.PunchEffect_2;
    protected StateController stateController;
    public StateController StateController => stateController;
    protected Cooldown dodgeCooldown = new Cooldown();
    public bool IsHealthRecovering { get; set; } = false;
    public bool IsHealthRecoveryInterrupted { get; set; } = false;
    public DashShadowEffect DashShadowEffect { get; set; }
    public GhostEffect GhostEffect { get; set; }
    public DissolveEffect DissolveEffect { get; set; }
    public bool IsHitStateActive { get; set; } = false;
    private readonly HashSet<WaterVolume> waterVolumes = new HashSet<WaterVolume>();
    private WaterVolume currentWaterVolume;
    public bool CanBeAttacked { get; set; } = true; // Có thể bị tấn công, bên enemy sẽ kiểm tra biến này trước khi tấn công, nếu false thì không thể tấn công nhân vật này

    public PoolType PoolType => characterData?.characterPoolType ?? PoolType.None;

    #region Swimming
    public float WaterLevel
    {
        get
        {
            return currentWaterVolume != null ? currentWaterVolume.SurfaceLevel : float.NaN;
        }
    }

    public bool IsInWaterVolume => currentWaterVolume != null;

    public virtual bool IsBodyBelowWaterLevel()
    {
        float waterLevel = WaterLevel;
        if (float.IsNaN(waterLevel) || float.IsInfinity(waterLevel))
            return false;

        CharacterController controller = characterMovement != null ? characterMovement.CC : null;
        float bodyCenterY = controller != null
            ? transform.TransformPoint(controller.center).y
            : transform.position.y + 0.75f;

        return bodyCenterY < waterLevel;
    }

    public virtual bool IsSwimmingCandidate()
    {
        return currentWaterVolume != null && IsBodyBelowWaterLevel();
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        WaterVolume waterVolume = other.GetComponentInParent<WaterVolume>();
        if (waterVolume == null)
            return;

        waterVolumes.Add(waterVolume);
        currentWaterVolume = waterVolume;
    }

    private void OnTriggerExit(Collider other)
    {
        WaterVolume waterVolume = other.GetComponentInParent<WaterVolume>();
        if (waterVolume == null)
            return;

        waterVolumes.Remove(waterVolume);
        if (currentWaterVolume == waterVolume)
        {
            currentWaterVolume = null;
            foreach (WaterVolume remainingVolume in waterVolumes)
            {
                currentWaterVolume = remainingVolume;
                break;
            }
        }
    }

    protected override void LoadComponent()
    {
        if (characterAnimation == null)
            characterAnimation = GetComponent<CharacterAnimation>();
        if (characterMovement == null)
            characterMovement = GetComponent<CharacterMovement>();
        if (characterRotate == null)
            characterRotate = GetComponent<CharacterRotate>();
        if (visuals == null)
            visuals = transform.Find("Visuals").gameObject;
        if (characterVisual == null)
            characterVisual = visuals.transform.GetChild(0)?.gameObject;
        if (characterWeapon == null)
            characterWeapon = GetComponent<CharacterWeapon>();
        if (characterCombat == null)
            characterCombat = GetComponent<CharacterCombat>();
        if (characterLockTarget == null)
            characterLockTarget = GetComponent<CharacterLockTarget>();
        if (characterSkill == null)
            characterSkill = GetComponent<CharacterSkill>();
        if (characterInput == null)
            characterInput = GetComponent<CharacterInput>();
        if (characterHealth == null)
            characterHealth = GetComponent<CharacterHealth>();
        if (characterRecovery == null)
            characterRecovery = GetComponent<CharacterRecovery>();
        if (characterMeleeHitbox == null)
            characterMeleeHitbox = GetComponent<CharacterMeleeHitbox>();
        if (characterCinematic == null)
            characterCinematic = GetComponent<CharacterCinematic>();
        if (characterStat == null)
            characterStat = GetComponent<CharacterStat>();
        if (characterStamina == null)
            characterStamina = GetComponent<CharacterStamina>();
        if (characterMP == null)
            characterMP = GetComponent<CharacterMP>();
        if (characterLevel == null)
            characterLevel = GetComponent<CharacterLevel>();
        if (characterGoldFalling == null)
            characterGoldFalling = GetComponent<CharacterGoldFalling>();
        if (characterRelic == null)
            characterRelic = GetComponent<CharacterRelic>();
        if (characterSound == null)
            characterSound = GetComponent<CharacterSound>();
        if (dustEffect == null)
            dustEffect = transform.Find("DustEffect")?.GetComponent<ParticleSystem>();
        LoadEffectPoints();
    }

    protected override void LoadComponentRuntime()
    {

    }

    protected virtual void LoadEffectPoints()
    {
        if (effectPoints == null)
            effectPoints = transform.Find("EffectPoints")?.gameObject;
        if (effectPoints == null)
            return;

        if (middleEffectPoint == null)
            middleEffectPoint = effectPoints.transform.Find("MiddleEffectPoint").gameObject;
        if (bottomEffectPoint == null)
            bottomEffectPoint = effectPoints.transform.Find("BottomEffectPoint").gameObject;
        if (punchEffectPoint_1 == null)
            punchEffectPoint_1 = effectPoints?.transform.Find("PunchEffectPoint_1")?.gameObject;
        if (punchEffectPoint_2 == null)
            punchEffectPoint_2 = effectPoints?.transform.Find("PunchEffectPoint_2")?.gameObject;
        if (punchEffectPoint_3 == null)
            punchEffectPoint_3 = effectPoints?.transform.Find("PunchEffectPoint_3")?.gameObject;
        if (punchEffectPoint_4 == null)
            punchEffectPoint_4 = effectPoints?.transform.Find("PunchEffectPoint_4")?.gameObject;
    }
    #region Init Character And Reset

    [Button("Init Character Data")]
    public virtual void Init(CharacterData data)
    {
        characterData = data;

        try
        {
            characterInput.Init(this);
            characterRecovery.Init(this);
            characterAnimation.Init(characterVisual);
            characterWeapon.Init(this, handRight.transform);
            characterCombat.Init(this, InitAttackCombos(), InitPunchCombos());
            characterMeleeHitbox.Init(this);
            characterStat.Init(this, characterData.stats);
            characterStamina.Init(this);
            characterMP.Init(this);
            characterLevel.Init(this);
            characterGoldFalling.Init(this);
            stateController = new StateController();
            stateController.ChangeState(new IdleState(this));
            characterMovement.CC.enabled = true;

            //SECTION - Skill
            characterSkill?.Init(this, characterData.skill1Data, characterData.skill2Data, GetSkill_1(characterData.skill1Data), GetSkill_2(characterData.skill2Data));

            GetDashShadowEffect(characterVisual);
            GetGhostEffect(characterVisual);
            GetDissolveEffect(characterVisual);

            GoldManager.Instance?.ResetGold();
            WeaponInventorySystem.Instance?.Init(characterWeapon);
            WeaponInventorySystem.Instance.AddWeapon(characterData.weaponData);
            InteractionManager.Instance?.Init(this.transform);
            TrySetCamera();
            EventManager.Notify(GameEvent.OnPlayerSpawned, transform);
        }
        catch (Exception e)
        {
            Debug.LogError("Thiếu dữ liệu, vui lòng gán đầy đủ dữ liệu, biến cho nhân vật: " + e.Message);
        }

    }

    /// <summary>
    /// Reset trạng thái nhân vật về mặc định, dùng khi respawn hoặc khi nhân vật chết
    /// Không phải trả nhân vật về pool
    /// </summary>
    public void ResetRespawnCharacter()
    {
        stateController?.ChangeState(new IdleState(this));
        characterHealth?.ResetHealth();
        characterStamina?.ResetStamina();
        characterMP?.ResetMP();
        // characterRecovery?.ResetRecovery();
        characterCombat?.ResetCombo();
        CanBeAttacked = true;
        IsHitStateActive = false;
        characterLockTarget?.ForceUnlockTarget();
        GoldManager.Instance?.ResetGold();
        DissolveEffect?.ResetDefaultMaterial();
        InteractionManager.Instance?.ClearInteractableObjects();
    }

    public void OnSpawnFromPool()
    {

    }

    public void OnReturnToPool()
    {
        CharacterInput.ClearInput();
    }

    private async void TrySetCamera()
    {
        if (CameraManager.Instance == null)
        {
            await UniTask.WaitUntil(() => CameraManager.Instance != null);
        }
        CameraManager.Instance.SetCamera(CameraType.Normal, transform, transform);
    }
    #endregion

    protected virtual void Update()
    {
        if (!CheckAnyStateTransition())
            stateController?.currentState?.Update();

        characterMovement.SetMoveDirection(characterInput.MoveInput);
        if (characterInput.LockTarget)
            OnLockTarget();

        PlayDustEffect();
    }

    protected virtual void FixedUpdate()
    {
        stateController?.currentState?.FixedUpdate();
    }

    protected virtual bool CheckAnyStateTransition()
    {
        if (stateController == null || stateController.currentState == null)
            return false;
        // if (stateController.currentState is BirdRideState)
        //     return false;

        //Chuyển về FallState nếu đang ở trên không và bắt đầu rơi
        if (!CharacterMovement.IsGrounded && CharacterMovement.VerticalVelocity < CharacterMovement.FallThreshold)
        {
            stateController.ChangeState(new FallState(this));
            return true;
        }

        return false;
    }

    #region Move
    public virtual void MoveNormal()
    {
        float speed = characterStat.finalStats.speed;

        Vector3 rotationDirection = new Vector3(characterMovement.MoveDirection.x,
                                                0f,
                                                characterMovement.MoveDirection.y);

        if (!characterStamina.HasEnoughStamina(characterData.staminaCost.dodgeCost))
        {
            characterInput.DisableSprint();
        }

        if (characterInput.Walk)
        {
            characterMovement.Walk(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Walk", 0.1f);
            characterRotate.Rotate(rotationDirection);
            return;
        }

        if (characterInput.Sprint)
        {
            ConsumeStaminaForSprint();
            characterMovement.Sprint(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Sprint", 0.1f);
            characterRotate.Rotate(rotationDirection);
            return;
        }

        if (!characterInput.Sprint || !characterInput.Walk)
        {
            characterMovement.Run(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Run", 0.1f);
            characterRotate.Rotate(rotationDirection);
            return;
        }
    }

    public virtual void MoveLockTarget()
    {
        float x = characterInput.MoveInput.x; // Hướng đi ngang
        float y = characterInput.MoveInput.y; // Hướng đi dọc
        float yAbs = Mathf.Abs(y); // Ngưỡng y để xác định di chuyển chéo hay thẳng
        float speed = characterStat.finalStats.speed;

        if (x < 0 && yAbs < characterMovement.StrafeThreshold)
        {
            characterMovement.Run(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Run_Strafe_Left", 0.1f);
            return;
        }

        if (x > 0 && yAbs < characterMovement.StrafeThreshold)
        {
            characterMovement.Run(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Run_Strafe_Right", 0.1f);
            return;
        }
        if (y < 0)
        {
            characterMovement.Run(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Run_Backward", 0.1f);
            return;
        }

        if (y > 0)
        {
            characterMovement.Run(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Run", 0.1f);
            return;
        }
    }

    protected virtual void ConsumeStaminaForSprint()
    {
        if (characterStamina.HasEnoughStamina(characterStat.finalStats.stamina) &&
                 characterInput.Sprint &&
                 characterInput.MoveInput != Vector2.zero)
        {
            characterStamina.SubtractStaminaOverTime(characterData.staminaCost.sprintCost);
        }
    }
    #endregion
    public virtual void Dodge()
    {
        if (dodgeCooldown.IsOnCooldown)
            return;

        if (!characterStamina.HasEnoughStamina(characterData.staminaCost.dodgeCost))
            return;

        characterStamina.SubtractStamina(characterData.staminaCost.dodgeCost);

        dodgeCooldown.StartCooldown(characterMovement.DodgeCooldown);

        stateController.ChangeState(new DodgeState(this));
    }

    #region Attack 
    public virtual void Attack()
    {
        if (!CheckStaminaAndMPForAttack())
            return;

        characterCombat?.TryAttack();
    }

    protected virtual bool CheckStaminaAndMPForAttack()
    {
        if (characterData.characterTypes == CharacterTypes.PhysicalMelee ||
            characterWeapon.CurrentWeapon == null) // Nếu là nhân vật vật lý hoặc không cầm vũ khí thì kiểm tra stamina
        {
            if (!characterStamina.HasEnoughStamina(characterData.staminaCost.attackCost))
                return false;

            characterStamina.SubtractStamina(characterData.staminaCost.attackCost);
            return true;
        }

        if (characterData.characterTypes == CharacterTypes.Magical) // Nếu là nhân vật phép thuật thì kiểm tra mana
        {
            if (!characterMP.HasEnoughMP(characterData.mpCost.attackCost))
            {
                return false;
            }


            characterMP.SubtractMP(characterData.mpCost.attackCost);
            return true;
        }

        return true;
    }

    /// <summary>
    /// Khởi tạo các đòn tấn công, bắt buộc phải override
    /// </summary>
    /// <returns></returns>
    protected virtual IAttackStep[] InitAttackCombos()
    {
        return null;
    }

    // Dùng để set punch combo khác cho nhân vật cụ thể
    protected virtual IAttackStep[] InitPunchCombos()
    {
        return null;
    }
    #endregion

    #region Skill

    protected virtual ICharacterSkill GetSkill_1(CharacterSkillData skill1Data)
    {
        return null;
    }

    protected virtual ICharacterSkill GetSkill_2(CharacterSkillData skill2Data)
    {
        return null;
    }

    public virtual void Skill_1()
    {
        characterSkill?.UseSkill1();

    }

    public virtual void Skill_2()
    {
        characterSkill?.UseSkill2();
    }

    // Năng lượng tiêu hao khi sử dụng kỹ năng, có thể là mana hoặc stamina, tùy thuộc vào thiết kế của từng nhân vật
    public virtual bool ConsumeSkillCost(CharacterTypes characterType, float cost)
    {
        if (characterType == CharacterTypes.Magical)
        {
            if (!characterMP.HasEnoughMP(cost))
            {
                return false;
            }
            characterMP.SubtractMP(cost);
            return true;
        }
        else if (characterType == CharacterTypes.PhysicalMelee)
        {
            if (!characterStamina.HasEnoughStamina(cost))
                return false;
            characterStamina.SubtractStamina(cost);
            return true;
        }
        return false;
    }
    #endregion

    /// <summary>
    /// Bật khoá mục tiêu, nếu đang khoá thì tắt, nếu đang tắt thì bật
    /// </summary>
    protected virtual void OnLockTarget()
    {
        if (CharacterLockTarget == null)
            return;
        CharacterLockTarget.ToggleLockTarget();
    }

    //Nhìn về phía mục tiêu khi đang khóa mục tiêu
    public virtual void LookAtTarget()
    {
        if (!characterLockTarget.IsLockingTarget)
            return;
        characterRotate.LookAt(characterLockTarget.Target.position);
    }

    #region Take Damage
    [Button("Take Damage (Test)")]
    public virtual void TakeDamage(DamageInfo damageInfo)
    {
        if (!CanBeAttacked) // Nếu nhân vật không thể bị tấn công, bỏ qua
            return;

        if (characterHealth.CurrentHealth <= 0)
            return;

        float finalDamage = damageInfo.damageAmount - characterStat.finalStats.defense; // Giảm sát thương dựa trên chỉ số phòng thủ
        finalDamage = Mathf.Max(finalDamage, minTakeDamage); // Đảm bảo sát thương không bị âm
        characterHealth.SubtractHealth(finalDamage);

        if (!damageInfo.isFromSafeZoneEffect) // Nếu không ở ngoài vùng an toàn
        {
            if (stateController?.currentState is HealthRecoveryState)
            {
                IsHealthRecoveryInterrupted = true;
            }

            characterMovement.KnockBack(damageInfo.attacker);
            CameraShake.Instance?.Shake();
            if (!IsHitStateActive || stateController?.currentState is HealthRecoveryState)
                stateController.ChangeState(new HitState(this));
        }

        if (characterHealth.CurrentHealth <= 0)
        {
            Die();
        }

        characterCombat.ResetCombo();
    }

    #endregion

    protected virtual void Die()
    {
        CanBeAttacked = false;
        stateController.ChangeState(new DeathState(this));
    }

    protected virtual void GetDashShadowEffect(GameObject characterVisual)
    {
        DashShadowEffect = characterVisual.GetComponent<DashShadowEffect>();
    }

    public virtual void GetGhostEffect(GameObject characterVisual)
    {
        GhostEffect = characterVisual.GetComponent<GhostEffect>();
    }

    public virtual void GetDissolveEffect(GameObject characterVisual)
    {
        DissolveEffect = characterVisual.GetComponent<DissolveEffect>();
    }

    /// <summary>
    /// Giúp kiểm tra xem trước mặt có kẻ địch nào gần không
    /// Dùng để tắt root motion khi tấn công nếu có kẻ địch gần, tránh trường hợp nhân vật bị kéo lùi lại quá xa khi tấn công mà không trúng mục tiêu nào
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckForNearEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * ZoffsetCheckForNearEnemy, radiusCheckForNearEnemy, enemyLayer);

        if (hitColliders.Length > 0) // Nếu có ít nhất 1 enemy thì true
        {
            return true;
        }
        return false;
    }

    #region Change Weapon

    public virtual void ChangeWeapon(InputAction.CallbackContext context)
    {
        if (UIManager.Instance.CurrentMenuType != MenuType.GameplayMenu &&
             UIManager.Instance.CurrentMenuType != MenuType.DefaultLobbyInputMenu)
            return;

        Vector2 scrollDelta = context.ReadValue<Vector2>();
        float scrollY = scrollDelta.y;

        if (scrollY > 0f)
        {
            if (CharacterInput.IsChangingWeapon ||
            CharacterInput.LockInput ||
             WeaponInventorySystem.Instance.CheckWeaponInSlots() == false ||
             characterCombat.IsAttacking ||
             characterSkill.IsUsingSkill
             ) return;

            StateController.ChangeState(new ChangeWeaponState(this));
        }
    }
    #endregion

    #region Effect

    protected virtual void PlayDustEffect()
    {
        if (dustEffect == null)
            return;

        if (characterMovement.IsGrounded && characterInput.IsMoving && !isDustEffectPlaying)
        {
            dustEffect.Play();
            isDustEffectPlaying = true;
        }
        else if ((!characterMovement.IsGrounded || !characterInput.IsMoving) && isDustEffectPlaying)
        {
            dustEffect.Stop();
            isDustEffectPlaying = false;
        }
    }

    #endregion

    #region Áp sát mục tiêu khi tấn công
    //Hỗ trợ áp sát mục tiêu khi tấn công
    protected void MeleeSnapToTarget()
    {
        if (CharacterLockTarget == null ||
        !CharacterLockTarget.IsLockingTarget ||
        !characterMovement.IsGrounded) // Chỉ áp sát nếu đang khóa mục tiêu
            return;

        Transform target = CharacterLockTarget.Target;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < meleeSnapThreshold.x || distanceToTarget > meleeSnapThreshold.y) return; // Nếu mục tiêu quá gần hoặc quá xa, không áp sát

        LungeToTarget();
    }

    protected virtual async void LungeToTarget()
    {
        CharacterMovement.IsLunging = true;

        float distanceToTarget = Vector3.Distance(transform.position, CharacterLockTarget.Target.position);

        while (distanceToTarget > meleeSnapThreshold.x && (characterMovement.flags & CollisionFlags.Sides) == 0) // Dừng khi đạt khoảng cách tối thiểu hoặc va chạm với tường
        {
            Vector3 directionToTarget = (CharacterLockTarget.Target.position - transform.position).normalized;
            CharacterMovement.Lunge(directionToTarget, characterData.stats.speed);

            distanceToTarget = Vector3.Distance(transform.position, CharacterLockTarget.Target.position);
            await UniTask.Yield();
        }
        CharacterMovement.Stop();
        CharacterMovement.IsLunging = false;
    }

    #endregion
}