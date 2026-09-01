using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AutoNPCWander : MonoBehaviour
{
    [System.Serializable]
    public class FixedRestPoint
    {
        public string pointName = "POI";
        public Transform pointTransform;
        public float restDuration = 5f;
    }

    [Header("0. Stationary NPC Setting")]
    [Tooltip("Tick chọn nếu NPC này là chủ tiệm đứng yên một chỗ, không bao giờ đi tuần tra")]
    [SerializeField] private bool isStationary = false;

    [Header("1. Key Locations / Points of Interest")]
    [SerializeField] private List<FixedRestPoint> keyRestPoints = new List<FixedRestPoint>();

    [Header("2. Random Roaming Settings")]
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float minTravelDistance = 4f;
    [SerializeField] private float minRandomRestTime = 3f;
    [SerializeField] private float maxRandomRestTime = 6f;

    [Header("3. Movement & Rotation Smoothness")]
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float questMoveSpeed = 5.5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float restTurnSpeed = 8f;
    [SerializeField] private float interactTurnSpeed = 20f;
    [SerializeField] private float acceleration = 16f;
    [SerializeField] private float stoppingDistance = 0.35f;

    [Header("4. Animation States")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string walkStateName = "Walk";
    [SerializeField] private float transitionDuration = 0.1f;

    private NavMeshAgent agent;
    private NPCQuestHandler questHandler;
    private Vector3 initialSpawnPos;
    private Quaternion initialSpawnRot;
    private float currentWaitTimer = 0f;
    private bool isWaiting = false;
    private bool isInteracting = false;
    private bool isMoving = false;
    private bool hasTriggeredInteractUnlock = false;

    private Transform playerTransform;
    private Quaternion targetRestRotation;
    private bool hasSpecificRestRotation = false;

    private int idleHash;
    private int walkHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        questHandler = GetComponent<NPCQuestHandler>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        initialSpawnRot = transform.rotation;

        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        idleHash = Animator.StringToHash(idleStateName);
        walkHash = Animator.StringToHash(walkStateName);
    }

    private void Start()
    {
        SnapToNavMesh();
        FindPlayer();
        
        if (isStationary)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            PlayAnimation(false);
            return;
        }

        if (questHandler != null && questHandler.ShouldStandAtStation())
        {
            Transform stationPoint = questHandler.GetQuestStationPoint();
            if (stationPoint != null && agent != null)
            {
                if (NavMesh.SamplePosition(stationPoint.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    transform.position = hit.position;
                    transform.rotation = stationPoint.rotation;
                }
            }

            ForceStationMode();
            StandAtQuestStation();
            UnlockNearbyInteractObjects();
        }
        else
        {
            PickNextSmartDestination();
        }
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += CheckQuestStateChange;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= CheckQuestStateChange;
        }
    }

    private void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            initialSpawnPos = hit.position;
            if (agent != null)
            {
                agent.Warp(hit.position);
            }
        }
        else
        {
            initialSpawnPos = transform.position;
        }
    }

    private Transform FindPlayer()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
        return playerTransform;
    }

    private void CheckQuestStateChange()
    {
        if (questHandler != null && questHandler.ShouldStandAtStation())
        {
            ForceStationMode();
        }
    }

    private void ForceStationMode()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        isWaiting = false;
        hasSpecificRestRotation = false;
        hasTriggeredInteractUnlock = false;
    }

    private void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        CheckInteractionState();

        // 1. ƯU TIÊN TUYỆT ĐỐI: Đang tương tác/hội thoại -> Dừng agent và quay mặt về phía Player
        if (isInteracting)
        {
            ForceLookAtPlayer();
            return;
        }

        // 2. Nếu là NPC đứng yên cố định
        if (isStationary)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, initialSpawnRot, restTurnSpeed * Time.deltaTime);
            PlayAnimation(false);
            return;
        }

        // 3. Nếu đang có Quest chỉ định đứng ở trạm
        if (questHandler != null && questHandler.ShouldStandAtStation())
        {
            StandAtQuestStation();
            return;
        }

        // 4. Chu trình tuần tra bình thường
        HandlePatrolLoop();
    }

    /// <summary>
    /// Ép Agent nhường quyền điều khiển Rotation và xoay mượt mặt NPC về phía người chơi
    /// </summary>
    private void ForceLookAtPlayer()
    {
        Transform player = FindPlayer();
        if (player == null) return;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
            agent.velocity = Vector3.zero;
        }

        PlayAnimation(false);

        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // Khóa trục Y để NPC không bị ngửa hoặc chúi đầu

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interactTurnSpeed * Time.deltaTime);
        }
    }

    private void StandAtQuestStation()
    {
        Transform stationPoint = questHandler != null ? questHandler.GetQuestStationPoint() : null;
        Vector3 targetStationPos = (stationPoint != null) ? stationPoint.position : initialSpawnPos;
        Quaternion targetStationRot = (stationPoint != null) ? stationPoint.rotation : initialSpawnRot;

        agent.speed = questMoveSpeed;

        float dist = Vector3.Distance(transform.position, targetStationPos);

        if (!hasTriggeredInteractUnlock)
        {
            hasTriggeredInteractUnlock = true;
            UnlockNearbyInteractObjects();
        }

        if (dist > stoppingDistance + 0.5f)
        {
            if (agent.isStopped || !agent.hasPath || agent.destination != targetStationPos)
            {
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(targetStationPos);
            }
            PlayAnimation(true);
        }
        else
        {
            if (!agent.isStopped || agent.hasPath)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.updateRotation = false;
            }

            PlayAnimation(false);

            if (!isInteracting)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetStationRot, restTurnSpeed * Time.deltaTime);
            }
        }
    }

    private void UnlockNearbyInteractObjects()
    {
        InteractV2[] allInteracts = FindObjectsByType<InteractV2>(FindObjectsSortMode.None);
        for (int i = 0; i < allInteracts.Length; i++)
        {
            if (allInteracts[i] != null)
            {
                allInteracts[i].CheckFeatureUnlockStatus();
            }
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.RescanNearbyInteracts();
        }
    }

    private void HandlePatrolLoop()
    {
        agent.speed = moveSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.updateRotation = false;
                PlayAnimation(false);
            }
            else
            {
                if (hasSpecificRestRotation)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRestRotation, restTurnSpeed * Time.deltaTime);
                }

                currentWaitTimer -= Time.deltaTime;
                if (currentWaitTimer <= 0f)
                {
                    agent.updateRotation = true;
                    PickNextSmartDestination();
                }
            }
        }
        else
        {
            agent.updateRotation = true;
            bool moving = agent.velocity.sqrMagnitude > 0.05f && !agent.isStopped;
            PlayAnimation(moving);
        }
    }

    private void CheckInteractionState()
    {
        FindPlayer();

        // 1. Kiểm tra đối thoại UI có đang mở không
        bool isDialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen();

        // 2. Kiểm tra xem người chơi có đang tương tác với chính NPC này không
        if (InteractManagerV2.Instance != null)
        {
            InteractV2 currentFocus = InteractManagerV2.Instance.CurrentInteract;
            bool isCurrentTarget = currentFocus != null && (currentFocus.gameObject == gameObject || currentFocus.transform.IsChildOf(transform));

            if (isCurrentTarget && (Input.GetKeyDown(KeyCode.F) || InteractManagerV2.Instance.IsBusy || isDialogueActive))
            {
                if (!isInteracting)
                {
                    PausePatrol(playerTransform);
                }
            }
        }

        // 3. Nếu đang tương tác nhưng hội thoại đã tắt và không còn bận -> Tiếp tục tuần tra
        if (isInteracting)
        {
            bool isMenuOpen = UIManager.Instance != null &&
                              UIManager.Instance.CurrentMenuType != MenuType.DefaultLobbyInputMenu &&
                              UIManager.Instance.CurrentMenuType != MenuType.GameplayMenu &&
                              UIManager.Instance.CurrentMenuType != MenuType.None;
            bool isManagerBusy = InteractManagerV2.Instance != null && InteractManagerV2.Instance.IsBusy;

            if (!isDialogueActive && !isMenuOpen && !isManagerBusy)
            {
                ResumePatrol();
            }
        }
    }

    private void PickNextSmartDestination()
    {
        if (questHandler != null && questHandler.ShouldStandAtStation())
        {
            return;
        }

        isWaiting = false;
        hasSpecificRestRotation = false;
        agent.speed = moveSpeed;

        if (keyRestPoints != null && keyRestPoints.Count > 0 && UnityEngine.Random.value < 0.55f)
        {
            var validPoints = keyRestPoints.FindAll(p => p.pointTransform != null && Vector3.Distance(transform.position, p.pointTransform.position) >= minTravelDistance);
            if (validPoints.Count > 0)
            {
                var chosenPOI = validPoints[UnityEngine.Random.Range(0, validPoints.Count)];
                if (TrySetDestination(chosenPOI.pointTransform.position))
                {
                    currentWaitTimer = chosenPOI.restDuration;

                    Vector3 euler = chosenPOI.pointTransform.eulerAngles;
                    targetRestRotation = Quaternion.Euler(0f, euler.y, 0f);
                    hasSpecificRestRotation = true;
                    return;
                }
            }
        }

        for (int attempts = 0; attempts < 15; attempts++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minTravelDistance, wanderRadius);
            Vector3 candidatePos = initialSpawnPos + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (Vector3.Distance(transform.position, candidatePos) < minTravelDistance)
                continue;

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 3.5f, NavMesh.AllAreas))
            {
                if (TrySetDestination(hit.position))
                {
                    currentWaitTimer = UnityEngine.Random.Range(minRandomRestTime, maxRandomRestTime);
                    hasSpecificRestRotation = false;
                    return;
                }
            }
        }

        isWaiting = true;
        currentWaitTimer = 2f;
        PlayAnimation(false);
    }

    private bool TrySetDestination(Vector3 targetPos)
    {
        if (agent == null || !agent.isOnNavMesh) return false;

        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(targetPos, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetPath(path);
            PlayAnimation(true);
            return true;
        }
        return false;
    }

    private void PlayAnimation(bool shouldMove)
    {
        if (animator == null) return;
        if (isMoving == shouldMove) return;

        isMoving = shouldMove;
        animator.CrossFade(isMoving ? walkHash : idleHash, transitionDuration);
    }

    /// <summary>
    /// Gọi trực tiếp từ SendMessage khi ấn F tương tác
    /// </summary>
    public void OnInteract()
    {
        PausePatrol(FindPlayer());
    }

    public void PausePatrol(Transform targetPlayer = null)
    {
        isInteracting = true;
        playerTransform = targetPlayer != null ? targetPlayer : FindPlayer();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.updateRotation = false;
        }

        PlayAnimation(false);
    }

    public void ResumePatrol()
    {
        if (!isInteracting) return;

        isInteracting = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            agent.updateRotation = false;
        }

        PlayAnimation(false);

        if (isStationary)
        {
            return;
        }

        if (questHandler != null && questHandler.ShouldStandAtStation())
        {
            StandAtQuestStation();
            return;
        }

        PickNextSmartDestination();
    }
}