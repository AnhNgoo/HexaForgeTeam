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
public abstract class CharacterBase : LoadComponents, ITakeDamage
{
    [Header("Character Data")]
    [SerializeField] protected CharacterData characterData;
    public CharacterData CharacterData => characterData;
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
    public DashShadowEffect dashShadowEffect { get; set; }

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
    #region Init Character

    //Test
    protected override void Awake()
    {
        base.Awake();

    }
    [Button("Init Character Data")]
    protected virtual void Init(CharacterData data)
    {
        if (data != null)
            characterData = Instantiate(data);

        try
        {
            CharacterInput.Init(this);
            characterHealth.Init(characterData.stats.maxHealth);
            characterRecovery.Init(this);
            characterAnimation.Init(characterVisual);
            characterWeapon.Init(this, handRight.transform);
            characterCombat.Init(this, InitAttackCombos(), InitPunchCombos());
            characterMeleeHitbox.Init(this);
            InitSkills();
            GetDashShadowEffect(characterVisual);
            EquipmentSystem.Instance?.Init(characterWeapon);
            InteractionManager.Instance?.Init(this.transform);
            CameraManager.Instance.SetCamera(CameraType.Normal, transform, transform);
        }
        catch (Exception e)
        {
            Debug.LogError("Thiếu dữ liệu, vui lòng gán đầy đủ dữ liệu, biến cho nhân vật: " + e.Message);
        }

    }

    #endregion
    protected virtual void Start()
    {
        Init(characterData);
        stateController = new StateController();
        stateController.ChangeState(new IdleState(this));
    }

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
        //Chuyển về FallState nếu đang ở trên không và bắt đầu rơi
        if (!CharacterMovement.IsGrounded && CharacterMovement.CC.velocity.y < CharacterMovement.FallThreshold)
        {
            StateController.ChangeState(new FallState(this));
            return true;
        }


        return false;
    }

    #region Move
    public virtual void MoveNormal()
    {
        float speed = characterData.stats.speed;

        Vector3 rotationDirection = new Vector3(characterMovement.MoveDirection.x,
                                                0f,
                                                characterMovement.MoveDirection.y);

        if (characterInput.Walk)
        {
            characterMovement.Walk(characterMovement.MoveDirection, speed);
            characterAnimation.CrossFadeOneshot("Walk", 0.1f);
            characterRotate.Rotate(rotationDirection);
            return;
        }

        if (characterInput.Sprint)
        {
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
        float speed = characterData.stats.speed;

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
    #endregion
    public virtual void Dodge()
    {
        if (dodgeCooldown.IsOnCooldown)
            return;

        dodgeCooldown.StartCooldown(characterMovement.DodgeCooldown);

        stateController.ChangeState(new DodgeState(this));
    }

    #region Attack 
    public virtual void Attack()
    {
        characterCombat?.TryAttack();
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
    protected virtual void InitSkills()
    {
        characterSkill?.Init(this, GetSkill_1(), GetSkill_2());
    }

    protected virtual ICharacterSkill GetSkill_1()
    {
        return null;
    }

    protected virtual ICharacterSkill GetSkill_2()
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

    #endregion
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
    public void TakeDamage(DamageInfo damageInfo)
    {
        float finalDamage = damageInfo.damageAmount - characterData.stats.defense; // Giảm sát thương dựa trên chỉ số phòng thủ
        finalDamage = Mathf.Max(finalDamage, 0); // Đảm bảo sát thương không bị âm
        characterHealth.SubtractHealth(finalDamage);

        if (!damageInfo.isFromSafeZoneEffect) // Nếu không ở ngoài vùng an toàn
        {
            characterMovement.KnockBack(damageInfo.attacker);
            stateController.ChangeState(new HitState(this));
            CameraShake.Instance?.Shake();
        }

        Debug.Log($"{gameObject.name} took {finalDamage} damage. Remaining health: {characterHealth.CurrentHealth}");

        if (characterHealth.CurrentHealth <= 0)
        {
            Die();
        }
    }

    #endregion

    private void Die()
    {
        stateController.ChangeState(new DeathState(this));
    }

    protected virtual void GetDashShadowEffect(GameObject characterVisual)
    {
        dashShadowEffect = characterVisual.GetComponent<DashShadowEffect>();
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
        Vector2 scrollDelta = context.ReadValue<Vector2>();
        float scrollY = scrollDelta.y;

        if (scrollY > 0f)
        {
            if (CharacterInput.IsChangingWeapon ||
             EquipmentSystem.Instance.GetWeaponCount() == 0 ||
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

    protected virtual void OnDrawGizmos()
    {
        if (debugModeCheckForNearEnemy)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + transform.forward * ZoffsetCheckForNearEnemy, radiusCheckForNearEnemy);
        }
    }
}