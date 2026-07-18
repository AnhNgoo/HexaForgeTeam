using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "EnemyPounceBiteSkillSO", menuName = "Enemy/Skills/Pounce Bite")]
public class EnemyPounceBiteSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private float pounceDistance = 2.5f; //Khoảng cách mà enemy sẽ nhảy tới khi sử dụng kỹ năng này, có thể dùng để điều chỉnh phạm vi tấn công của kỹ năng này
    [SerializeField] private float stopDistance = 1.3f; //Khoảng cách mà enemy sẽ dừng lại khi nhảy tới gần player, có thể dùng để điều chỉnh khoảng cách an toàn giữa enemy và player sau khi nhảy
    [SerializeField] private float pounceDuration = 0.5f; //Thời gian mà enemy sẽ mất để hoàn thành cú nhảy, có thể dùng để điều chỉnh tốc độ của cú nhảy và thời gian mà player có thể phản ứng
    [SerializeField] private float navMeshSampleRadius = 1f; //Bán kính để kiểm tra vị trí hợp lệ trên NavMesh khi nhảy, có thể dùng để đảm bảo rằng enemy sẽ nhảy tới một vị trí có thể di chuyển được trên NavMesh
    [SerializeField] private float minimumPounceMoveDistance = 0.75f; //Khoảng cách tối thiểu mà enemy phải di chuyển khi nhảy, nếu khoảng cách từ enemy đến điểm đích nhỏ hơn giá trị này, enemy sẽ không thực hiện cú nhảy để tránh việc nhảy
    [SerializeField] private float trackingTime = 0.18f; //Thời gian mà enemy sẽ theo dõi vị trí của player trước khi nhảy, có thể dùng để tạo ra một khoảng thời gian mà player có thể phản ứng trước khi enemy nhảy
    [SerializeField] private float turnSpeed = 900f; //Tốc độ mà enemy sẽ xoay để hướng về phía player trước khi nhảy, có thể dùng để điều chỉnh tốc độ xoay của enemy và tạo ra một chuyển động nhảy mượt mà hơn
    [SerializeField] private AnimationCurve pounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); //Đường cong để điều chỉnh chuyển động của cú nhảy, có thể dùng để tạo ra một chuyển động nhảy mượt mà và tự nhiên hơn

    public override void OnAttackMovement(EnemyAttackContext context)
    {
        base.OnAttackMovement(context);

        if (context == null || context.Enemy == null || context.AttackData == null) return;

        PounceAsync(context).Forget(); //Bắt đầu quá trình nhảy tới player khi cú nhảy chạm đất để đảm bảo rằng enemy sẽ thực hiện cú nhảy đúng thời điểm, tránh lỗi nhảy không đúng lúc trong animation tấn công
    }

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        base.OnAttackImpact(context);

        if (context == null || context.Enemy == null || context.AttackData == null) return;

        FaceTargetInstant(context.Enemy, context.Target); //Xoay enemy ngay lập tức về phía player khi cú nhảy chạm đất để đảm bảo rằng enemy sẽ hướng về phía player đúng thời điểm, tránh lỗi xoay không đúng lúc trong animation tấn công
        context.Enemy.Combat.EnableHitbox(context.AttackData.hitboxType);//Kích hoạt hitbox của đòn tấn công ngay khi cú nhảy chạm đất để đảm bảo rằng hitbox sẽ được kích hoạt đúng thời điểm, tránh lỗi hitbox không mở khi animation tấn công đang diễn ra
    }

    public override void OnAttackEnd(EnemyAttackContext context)
    {
        base.OnAttackEnd(context);

        if (context == null || context.Enemy == null || context.AttackData == null) return;

        context.Enemy.Combat.DisableHitbox(context.AttackData.hitboxType); //Vô hiệu hóa hitbox của đòn tấn công khi kết thúc đòn tấn công để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
    }

    private void FaceTargetInstant(EnemyBase enemy, Transform target)
    {
        if (enemy == null || target == null) return;

        Vector3 direction = target.position - enemy.MyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f) return;

        enemy.MyTransform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private async UniTask PounceAsync(EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        Transform target = context.Target;

        Vector3 start = enemy.MyTransform.position;
        float lockedY = enemy.MyTransform.position.y;
        Vector3 toTarget = target.position - start;
        toTarget.y = 0; //Giữ nguyên chiều cao để nhảy theo phương ngang

        float distance = toTarget.magnitude;
        if (distance <= stopDistance) return; //Nếu đã ở trong khoảng cách dừng, không cần nhảy

        Vector3 direction = toTarget.normalized;

        float moveDistance = Mathf.Min(pounceDistance, distance - stopDistance);
        if (moveDistance < minimumPounceMoveDistance) return; //Nếu khoảng cách di chuyển nhỏ hơn khoảng cách tối thiểu, không thực hiện cú nhảy

        Vector3 desiredEnd = start + direction * moveDistance;

        desiredEnd = enemy.Detection.ClampPointToLeash(desiredEnd);

        if (NavMesh.SamplePosition(desiredEnd, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas)) //Kiểm tra nếu vị trí mong muốn hợp lệ trên NavMesh, nếu không thì tìm vị trí gần nhất trên NavMesh
        {
            desiredEnd.x = hit.position.x;
            desiredEnd.z = hit.position.z;
            desiredEnd.y = lockedY; //Giữ nguyên chiều cao đã khóa để đảm bảo rằng enemy sẽ nhảy theo phương ngang
        }

        enemy.Locomotion.StopMoving();
        enemy.Locomotion.SetAgentActive(false);

        float timer = 0f;

        while (timer < pounceDuration)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) return;

            timer += Time.deltaTime;

            if (timer <= trackingTime && target != null)
            {
                Vector3 liveToTarget = target.position - start;
                liveToTarget.y = 0f;

                if (liveToTarget.sqrMagnitude > 0.01f)
                {
                    direction = liveToTarget.normalized;

                    float liveDistance = liveToTarget.magnitude;
                    float liveMoveDistance = Mathf.Min(pounceDistance, liveDistance - stopDistance);

                    if (liveMoveDistance > 0f)
                    {
                        desiredEnd = start + direction * liveMoveDistance;
                        desiredEnd = enemy.Detection.ClampPointToLeash(desiredEnd);

                        if (NavMesh.SamplePosition(desiredEnd, out NavMeshHit liveHit, navMeshSampleRadius, NavMesh.AllAreas))
                        {
                            desiredEnd.x = liveHit.position.x;
                            desiredEnd.z = liveHit.position.z;
                            desiredEnd.y = lockedY;
                        }
                    }

                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    enemy.MyTransform.rotation = Quaternion.RotateTowards(
                        enemy.MyTransform.rotation,
                        targetRotation,
                        turnSpeed * Time.deltaTime
                    );
                }
            }

            float t = Mathf.Clamp01(timer / pounceDuration); //Chuẩn hóa thời gian để phù hợp với khoảng từ 0 đến 1, có thể dùng để đánh giá đường cong chuyển động của cú nhảy
            float curveT = pounceCurve.Evaluate(t); //Đánh giá đường cong để điều chỉnh chuyển động của cú nhảy, có thể dùng để tạo ra một chuyển động nhảy mượt mà và tự nhiên hơn

            Vector3 nextPosition = Vector3.Lerp(start, desiredEnd, curveT); //Tính toán vị trí tiếp theo của enemy dựa trên đường thẳng từ điểm bắt đầu đến điểm kết thúc và điều chỉnh bằng đường cong để tạo ra một chuyển động nhảy mượt mà và tự nhiên hơn
            nextPosition.y = lockedY; //Giữ nguyên chiều cao đã khóa để đảm bảo rằng enemy sẽ nhảy theo phương ngang
            enemy.MyTransform.position = nextPosition; //Cập nhật vị trí của enemy để tạo ra chuyển động nhảy, có thể dùng để tăng tính thẩm mỹ của kỹ năng này

            if (direction.sqrMagnitude > 0.01f && timer > trackingTime) //Nếu có hướng di chuyển hợp lệ, xoay enemy theo hướng di chuyển để tạo ra một chuyển động nhảy tự nhiên hơn
            {
                enemy.MyTransform.rotation = Quaternion.LookRotation(direction); //Xoay enemy theo hướng di chuyển để tạo ra một chuyển động nhảy tự nhiên hơn, có thể dùng để tăng tính thẩm mỹ của kỹ năng này
            }

            await UniTask.Yield();
        }

        desiredEnd.y = lockedY;
        enemy.MyTransform.position = desiredEnd;
        enemy.Locomotion.SetAgentActive(true);
        enemy.Locomotion.WarpTo(desiredEnd);
        enemy.Locomotion.StopMoving();
    }
}
