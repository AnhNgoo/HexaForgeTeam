using UnityEngine;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine.Serialization;
using System;

namespace Cinemachine
{
    /// <summary>
    /// Tương tự CinemachineCollider nhưng thay vì bảo vệ line-of-sight đến LookAt target,
    /// nó bảo vệ line-of-sight đến FOLLOW target (player).
    /// Dùng khi LockTarget camera: Follow = Player, LookAt = Enemy.
    /// Camera sẽ zoom vào để thấy Player thay vì Enemy khi bị chắn.
    /// </summary>
    [AddComponentMenu("Cinemachine/Extensions/Follow Collider")]
    [SaveDuringPlay]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class CinemachineFollowCollider : CinemachineExtension
    {
        [Header("Obstacle Detection")]
        [Tooltip("Objects on these layers will be detected")]
        public LayerMask m_CollideAgainst = 1;

        [TagField]
        [Tooltip("Obstacles with this tag will be ignored.")]
        public string m_IgnoreTag = string.Empty;

        [Tooltip("Objects on these layers will never obstruct view of the Follow target")]
        public LayerMask m_TransparentLayers = 0;

        [Tooltip("Obstacles closer to the Follow target than this will be ignored")]
        public float m_MinimumDistanceFromTarget = 0.1f;

        [Space]
        [Tooltip("When enabled, will attempt to resolve situations where the line of sight to the FOLLOW target is blocked by an obstacle")]
        public bool m_AvoidObstacles = true;

        [Tooltip("The maximum raycast distance when checking if the line of sight to the Follow target is clear. 0 = use actual distance.")]
        public float m_DistanceLimit;

        [Tooltip("Don't take action unless occlusion has lasted at least this long.")]
        public float m_MinimumOcclusionTime;

        [Tooltip("Camera will try to maintain this distance from any obstacle.")]
        public float m_CameraRadius = 0.1f;

        public enum ResolutionStrategy
        {
            PullCameraForward,
            PreserveCameraHeight,
            PreserveCameraDistance
        };

        public ResolutionStrategy m_Strategy = ResolutionStrategy.PreserveCameraHeight;

        [Range(1, 10)]
        public int m_MaximumEffort = 4;

        [Range(0, 2)]
        public float m_SmoothingTime;

        [Range(0, 10)]
        [FormerlySerializedAs("m_Smoothing")]
        public float m_Damping;

        [Range(0, 10)]
        public float m_DampingWhenOccluded;

        void OnValidate()
        {
            m_DistanceLimit             = Mathf.Max(0, m_DistanceLimit);
            m_MinimumOcclusionTime      = Mathf.Max(0, m_MinimumOcclusionTime);
            m_CameraRadius              = Mathf.Max(0, m_CameraRadius);
            m_MinimumDistanceFromTarget = Mathf.Max(0.01f, m_MinimumDistanceFromTarget);
        }

        protected override void OnDestroy()
        {
            DestroyScratchCollider();
            base.OnDestroy();
        }

        // ── Scratch collider tự quản lý (thay thế RuntimeUtility) ────────────
        private static SphereCollider s_ScratchCollider;

        private static SphereCollider GetScratchCollider()
        {
            if (s_ScratchCollider == null)
            {
                var go = new GameObject("__CM_FollowCollider_Scratch__")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                go.SetActive(false);
                s_ScratchCollider = go.AddComponent<SphereCollider>();
            }
            return s_ScratchCollider;
        }

        private static void DestroyScratchCollider()
        {
            if (s_ScratchCollider == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(s_ScratchCollider.gameObject);
            else
#endif
                Destroy(s_ScratchCollider.gameObject);
            s_ScratchCollider = null;
        }

        // ── Helper: Raycast bỏ qua tag ───────────────────────────────────────
        private bool RaycastIgnoreTag(Ray ray, out RaycastHit hit, float distance, int layerMask)
        {
            if (string.IsNullOrEmpty(m_IgnoreTag))
                return Physics.Raycast(ray, out hit, distance, layerMask, QueryTriggerInteraction.Ignore);

            RaycastHit[] hits = Physics.RaycastAll(ray, distance, layerMask, QueryTriggerInteraction.Ignore);
            hit = default;
            float nearest = float.MaxValue;
            bool  found   = false;
            foreach (var h in hits)
            {
                if (h.collider.CompareTag(m_IgnoreTag)) continue;
                if (h.distance < nearest) { nearest = h.distance; hit = h; found = true; }
            }
            return found;
        }

        private bool SphereCastIgnoreTag(Vector3 origin, float radius, Vector3 dir,
            out RaycastHit hit, float distance, int layerMask)
        {
            if (string.IsNullOrEmpty(m_IgnoreTag))
                return Physics.SphereCast(origin, radius, dir, out hit, distance, layerMask, QueryTriggerInteraction.Ignore);

            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, dir, distance, layerMask, QueryTriggerInteraction.Ignore);
            hit = default;
            float nearest = float.MaxValue;
            bool  found   = false;
            foreach (var h in hits)
            {
                if (h.collider.CompareTag(m_IgnoreTag)) continue;
                if (h.distance < nearest) { nearest = h.distance; hit = h; found = true; }
            }
            return found;
        }

        // ─────────────────────────────────────────────────────────────────────

        const float k_PrecisionSlush = 0.001f;

        class VcamExtraState
        {
            public Vector3 previousDisplacement;
            public Vector3 previousCameraOffset;
            public Vector3 previousCameraPosition;
            public float   previousDampTime;
            public bool    targetObscured;
            public float   occlusionStartTime;
            public List<Vector3> debugResolutionPath;

            public void AddPointToDebugPath(Vector3 p)
            {
#if UNITY_EDITOR
                if (debugResolutionPath == null)
                    debugResolutionPath = new List<Vector3>();
                debugResolutionPath.Add(p);
#endif
            }

            float m_SmoothedDistance;
            float m_SmoothedTime;

            public float ApplyDistanceSmoothing(float distance, float smoothingTime)
            {
                if (m_SmoothedTime != 0 && smoothingTime > Epsilon)
                {
                    float now = CinemachineCore.CurrentTime;
                    if (now - m_SmoothedTime < smoothingTime)
                        return Mathf.Min(distance, m_SmoothedDistance);
                }
                return distance;
            }

            public void UpdateDistanceSmoothing(float distance)
            {
                if (m_SmoothedDistance == 0 || distance < m_SmoothedDistance)
                {
                    m_SmoothedDistance = distance;
                    m_SmoothedTime     = CinemachineCore.CurrentTime;
                }
            }

            public void ResetDistanceSmoothing(float smoothingTime)
            {
                float now = CinemachineCore.CurrentTime;
                if (now - m_SmoothedTime >= smoothingTime)
                    m_SmoothedDistance = m_SmoothedTime = 0;
            }
        }

        public override float GetMaxDampTime()
        {
            return Mathf.Max(m_Damping, Mathf.Max(m_DampingWhenOccluded, m_SmoothingTime));
        }

        public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
        {
            var states = GetAllExtraStates<VcamExtraState>();
            for (int i = 0; i < states.Count; ++i)
                states[i].previousCameraPosition += positionDelta;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            // ── Lấy Follow target thay vì LookAt ────────────────────────────────
            Transform followTarget  = vcam.Follow;
            bool      hasFollowTarget = followTarget != null;

            if (stage == CinemachineCore.Stage.Body)
            {
                var extra = GetExtraState<VcamExtraState>(vcam);
                extra.targetObscured = false;
                extra.debugResolutionPath?.RemoveRange(0, extra.debugResolutionPath.Count);

                if (m_AvoidObstacles && hasFollowTarget)
                {
                    Vector3 followPos     = followTarget.position;
                    var initialCamPos     = state.CorrectedPosition;
                    var dampingBypass     = Quaternion.Euler(state.PositionDampingBypass);
                    extra.previousDisplacement = dampingBypass * extra.previousDisplacement;

                    Vector3 displacement = PreserveLineOfSight(ref state, ref extra, followPos);

                    if (m_MinimumOcclusionTime > Epsilon)
                    {
                        float now = CinemachineCore.CurrentTime;
                        if (displacement == Vector3.zero)
                            extra.occlusionStartTime = 0;
                        else
                        {
                            if (extra.occlusionStartTime <= 0)
                                extra.occlusionStartTime = now;
                            if (now - extra.occlusionStartTime < m_MinimumOcclusionTime)
                                displacement = extra.previousDisplacement;
                        }
                    }

                    if (m_SmoothingTime > Epsilon)
                    {
                        Vector3 pos      = initialCamPos + displacement;
                        Vector3 dir      = pos - followPos;
                        float   distance = dir.magnitude;
                        if (distance > Epsilon)
                        {
                            dir /= distance;
                            if (displacement != Vector3.zero)
                                extra.UpdateDistanceSmoothing(distance);
                            distance    = extra.ApplyDistanceSmoothing(distance, m_SmoothingTime);
                            displacement += (followPos + dir * distance) - pos;
                        }
                    }

                    if (displacement == Vector3.zero)
                        extra.ResetDistanceSmoothing(m_SmoothingTime);

                    var cameraPos = initialCamPos + displacement;
                    displacement += RespectCameraRadius(cameraPos, followPos);

                    float dampTime = m_DampingWhenOccluded;
                    if (deltaTime >= 0 && VirtualCamera.PreviousStateIsValid
                        && m_DampingWhenOccluded + m_Damping > Epsilon)
                    {
                        var dispSqrMag = displacement.sqrMagnitude;
                        dampTime = dispSqrMag > extra.previousDisplacement.sqrMagnitude
                            ? m_DampingWhenOccluded : m_Damping;

                        if (dispSqrMag < Epsilon)
                            dampTime = extra.previousDampTime - Damper.Damp(extra.previousDampTime, dampTime, deltaTime);

                        if (dampTime > 0)
                        {
                            bool bodyAfterAim = false;
                            if (vcam is CinemachineVirtualCamera cvc)
                            {
                                var body = cvc.GetCinemachineComponent(CinemachineCore.Stage.Body);
                                bodyAfterAim = body != null && body.BodyAppliesAfterAim;
                            }
                            var prevDisp = bodyAfterAim
                                ? extra.previousDisplacement
                                : followPos + dampingBypass * extra.previousCameraOffset - initialCamPos;
                            displacement = prevDisp + Damper.Damp(displacement - prevDisp, dampTime, deltaTime);
                        }
                    }

                    state.PositionCorrection += displacement;
                    cameraPos = state.CorrectedPosition;

                    if (hasFollowTarget && VirtualCamera.PreviousStateIsValid)
                    {
                        var dir0 = extra.previousCameraPosition - followPos;
                        var dir1 = cameraPos - followPos;
                        if (dir0.sqrMagnitude > Epsilon && dir1.sqrMagnitude > Epsilon)
                            state.PositionDampingBypass = UnityVectorExtensions
                                .SafeFromToRotation(dir0, dir1, state.ReferenceUp).eulerAngles;
                    }

                    extra.previousDisplacement   = displacement;
                    extra.previousCameraOffset   = cameraPos - followPos;
                    extra.previousCameraPosition = cameraPos;
                    extra.previousDampTime       = dampTime;
                }
            }

            if (stage == CinemachineCore.Stage.Aim && hasFollowTarget)
            {
                var extra = GetExtraState<VcamExtraState>(vcam);
                extra.targetObscured = CheckForFollowTargetObstructions(state, followTarget.position);
                if (extra.targetObscured)
                    state.ShotQuality *= 0.2f;
                if (!extra.previousDisplacement.AlmostZero())
                    state.ShotQuality *= 0.8f;
            }
        }

        Vector3 PreserveLineOfSight(ref CameraState state, ref VcamExtraState extra, Vector3 followPos)
        {
            Vector3 displacement = Vector3.zero;
            if (m_CollideAgainst != 0 && m_CollideAgainst != m_TransparentLayers)
            {
                Vector3    cameraPos = state.CorrectedPosition;
                RaycastHit hitInfo   = new RaycastHit();
                displacement = PullCameraInFrontOfNearestObstacle(
                    cameraPos, followPos, m_CollideAgainst & ~m_TransparentLayers, ref hitInfo);

                Vector3 pos = cameraPos + displacement;
                if (hitInfo.collider != null)
                {
                    extra.AddPointToDebugPath(pos);
                    if (m_Strategy != ResolutionStrategy.PullCameraForward)
                    {
                        Vector3 targetToCamera = cameraPos - followPos;
                        pos = PushCameraBack(pos, targetToCamera, hitInfo, followPos,
                            new Plane(state.ReferenceUp, cameraPos),
                            targetToCamera.magnitude, m_MaximumEffort, ref extra);
                    }
                }
                displacement = pos - cameraPos;
            }
            return displacement;
        }

        Vector3 PullCameraInFrontOfNearestObstacle(
            Vector3 cameraPos, Vector3 followPos, int layerMask, ref RaycastHit hitInfo)
        {
            Vector3 displacement   = Vector3.zero;
            Vector3 dir            = cameraPos - followPos;
            float   targetDistance = dir.magnitude;

            if (targetDistance > Epsilon)
            {
                dir /= targetDistance;
                float minDist = Mathf.Max(m_MinimumDistanceFromTarget, Epsilon);

                if (targetDistance < minDist + Epsilon)
                {
                    displacement = dir * (minDist - targetDistance);
                }
                else
                {
                    float rayLength = targetDistance - minDist;
                    if (m_DistanceLimit > Epsilon)
                        rayLength = Mathf.Min(m_DistanceLimit, rayLength);

                    Ray ray = new Ray(cameraPos - rayLength * dir, dir);
                    rayLength += k_PrecisionSlush;

                    if (rayLength > Epsilon)
                    {
                        if (m_Strategy == ResolutionStrategy.PullCameraForward && m_CameraRadius >= Epsilon)
                        {
                            if (SphereCastIgnoreTag(followPos + dir * m_CameraRadius,
                                m_CameraRadius, dir, out hitInfo,
                                rayLength - m_CameraRadius, layerMask))
                            {
                                displacement = (hitInfo.point + hitInfo.normal * m_CameraRadius) - cameraPos;
                            }
                        }
                        else
                        {
                            if (RaycastIgnoreTag(ray, out hitInfo, rayLength, layerMask))
                            {
                                float adjustment = Mathf.Max(0, hitInfo.distance - k_PrecisionSlush);
                                displacement = ray.GetPoint(adjustment) - cameraPos;
                            }
                        }
                    }
                }
            }
            return displacement;
        }

        Vector3 PushCameraBack(Vector3 currentPos, Vector3 pushDir, RaycastHit obstacle,
            Vector3 followPos, Plane startPlane, float targetDistance, int iterations,
            ref VcamExtraState extra)
        {
            Vector3 pos = currentPos;
            Vector3 dir = Vector3.zero;
            if (!GetWalkingDirection(pos, pushDir, obstacle, ref dir)) return pos;

            Ray   ray      = new Ray(pos, dir);
            float distance = GetPushBackDistance(ray, startPlane, targetDistance, followPos);
            if (distance <= Epsilon) return pos;

            float clampedDist = ClampRayToBounds(ray, distance, obstacle.collider.bounds);
            distance = Mathf.Min(distance, clampedDist + k_PrecisionSlush);

            if (RaycastIgnoreTag(ray, out var hitInfo, distance, m_CollideAgainst & ~m_TransparentLayers))
            {
                pos = ray.GetPoint(hitInfo.distance - k_PrecisionSlush);
                extra.AddPointToDebugPath(pos);
                if (iterations > 1)
                    pos = PushCameraBack(pos, dir, hitInfo, followPos, startPlane,
                        targetDistance, iterations - 1, ref extra);
                return pos;
            }

            pos = ray.GetPoint(distance);
            dir = pos - followPos;
            float d = dir.magnitude;
            if (d < Epsilon || RaycastIgnoreTag(new Ray(followPos, dir), out _,
                    d - k_PrecisionSlush, m_CollideAgainst & ~m_TransparentLayers))
                return currentPos;

            ray = new Ray(pos, dir);
            extra.AddPointToDebugPath(pos);
            distance = GetPushBackDistance(ray, startPlane, targetDistance, followPos);
            if (distance > Epsilon)
            {
                if (!RaycastIgnoreTag(ray, out hitInfo, distance, m_CollideAgainst & ~m_TransparentLayers))
                {
                    pos = ray.GetPoint(distance);
                    extra.AddPointToDebugPath(pos);
                }
                else
                {
                    pos = ray.GetPoint(hitInfo.distance - k_PrecisionSlush);
                    extra.AddPointToDebugPath(pos);
                    if (iterations > 1)
                        pos = PushCameraBack(pos, dir, hitInfo, followPos, startPlane,
                            targetDistance, iterations - 1, ref extra);
                }
            }
            return pos;
        }

        RaycastHit[] m_CornerBuffer = new RaycastHit[4];

        bool GetWalkingDirection(Vector3 pos, Vector3 pushDir, RaycastHit obstacle, ref Vector3 outDir)
        {
            Vector3 normal2      = obstacle.normal;
            float   nearbyDist   = k_PrecisionSlush * 5;
            int     numFound     = Physics.SphereCastNonAlloc(pos, nearbyDist, pushDir.normalized,
                m_CornerBuffer, 0, m_CollideAgainst & ~m_TransparentLayers, QueryTriggerInteraction.Ignore);

            if (numFound > 1)
            {
                for (int i = 0; i < numFound; ++i)
                {
                    if (m_CornerBuffer[i].collider == null) continue;
                    if (!string.IsNullOrEmpty(m_IgnoreTag) && m_CornerBuffer[i].collider.CompareTag(m_IgnoreTag)) continue;
                    Type type = m_CornerBuffer[i].collider.GetType();
                    if (type == typeof(BoxCollider) || type == typeof(SphereCollider) || type == typeof(CapsuleCollider))
                    {
                        Vector3 p = m_CornerBuffer[i].collider.ClosestPoint(pos);
                        Vector3 dv = p - pos;
                        if (dv.magnitude > Vector3.kEpsilon)
                        {
                            if (m_CornerBuffer[i].collider.Raycast(new Ray(pos, dv), out m_CornerBuffer[i], nearbyDist))
                            {
                                if (!(m_CornerBuffer[i].normal - obstacle.normal).AlmostZero())
                                { normal2 = m_CornerBuffer[i].normal; break; }
                            }
                        }
                    }
                }
            }

            Vector3 walkDir = Vector3.Cross(obstacle.normal, normal2);
            if (walkDir.AlmostZero())
                walkDir = Vector3.ProjectOnPlane(pushDir, obstacle.normal);
            else
            {
                float dot = Vector3.Dot(walkDir, pushDir);
                if (Mathf.Abs(dot) < Epsilon) return false;
                if (dot < 0) walkDir = -walkDir;
            }
            if (walkDir.AlmostZero()) return false;
            outDir = walkDir.normalized;
            return true;
        }

        const float k_AngleThreshold = 0.1f;

        float GetPushBackDistance(Ray ray, Plane startPlane, float targetDistance, Vector3 followPos)
        {
            float maxDist = targetDistance - (ray.origin - followPos).magnitude;
            if (maxDist < Epsilon) return 0;
            if (m_Strategy == ResolutionStrategy.PreserveCameraDistance) return maxDist;
            if (!startPlane.Raycast(ray, out var dist)) dist = 0;
            dist = Mathf.Min(maxDist, dist);
            if (dist < Epsilon) return 0;
            float angle = Mathf.Abs(UnityVectorExtensions.Angle(startPlane.normal, ray.direction) - 90);
            if (angle < k_AngleThreshold)
                dist = Mathf.Lerp(0, dist, angle / k_AngleThreshold);
            return dist;
        }

        static float ClampRayToBounds(Ray ray, float distance, Bounds bounds)
        {
            float d;
            if (Vector3.Dot(ray.direction, Vector3.up) > 0)
            { if (new Plane(Vector3.down, bounds.max).Raycast(ray, out d) && d > Epsilon) distance = Mathf.Min(distance, d); }
            else if (Vector3.Dot(ray.direction, Vector3.down) > 0)
            { if (new Plane(Vector3.up, bounds.min).Raycast(ray, out d) && d > Epsilon) distance = Mathf.Min(distance, d); }
            if (Vector3.Dot(ray.direction, Vector3.right) > 0)
            { if (new Plane(Vector3.left, bounds.max).Raycast(ray, out d) && d > Epsilon) distance = Mathf.Min(distance, d); }
            else if (Vector3.Dot(ray.direction, Vector3.left) > 0)
            { if (new Plane(Vector3.right, bounds.min).Raycast(ray, out d) && d > Epsilon) distance = Mathf.Min(distance, d); }
            if (Vector3.Dot(ray.direction, Vector3.forward) > 0)
            { if (new Plane(Vector3.back, bounds.max).Raycast(ray, out d) && d > Epsilon) distance = Mathf.Min(distance, d); }
            else if (Vector3.Dot(ray.direction, Vector3.back) > 0)
            { if (new Plane(Vector3.forward, bounds.min).Raycast(ray, out d) && d > Epsilon) distance = Mathf.Min(distance, d); }
            return distance;
        }

        static Collider[] s_ColliderBuffer = new Collider[5];

        Vector3 RespectCameraRadius(Vector3 cameraPos, Vector3 followPos)
        {
            Vector3 result   = Vector3.zero;
            if (m_CameraRadius < Epsilon || m_CollideAgainst == 0) return result;

            Vector3 dir      = cameraPos - followPos;
            float   distance = dir.magnitude;
            if (distance > Epsilon) dir /= distance;

            RaycastHit hitInfo;
            int numObstacles = Physics.OverlapSphereNonAlloc(cameraPos, m_CameraRadius,
                s_ColliderBuffer, m_CollideAgainst, QueryTriggerInteraction.Ignore);

            if (numObstacles == 0 && m_TransparentLayers != 0
                && distance > m_MinimumDistanceFromTarget + Epsilon)
            {
                float   d         = distance - m_MinimumDistanceFromTarget;
                Vector3 targetPos = followPos + dir * m_MinimumDistanceFromTarget;
                if (RaycastIgnoreTag(new Ray(targetPos, dir), out hitInfo, d, m_CollideAgainst))
                {
                    Collider c = hitInfo.collider;
                    if (!c.Raycast(new Ray(cameraPos, -dir), out hitInfo, d))
                        s_ColliderBuffer[numObstacles++] = c;
                }
            }

            if (numObstacles > 0 && distance == 0 || distance > m_MinimumDistanceFromTarget)
            {
                var scratchCollider = GetScratchCollider();
                scratchCollider.radius = m_CameraRadius;

                Vector3 newCamPos = cameraPos;
                for (int i = 0; i < numObstacles; ++i)
                {
                    Collider c = s_ColliderBuffer[i];
                    if (!string.IsNullOrEmpty(m_IgnoreTag) && c.CompareTag(m_IgnoreTag)) continue;

                    if (distance > m_MinimumDistanceFromTarget)
                    {
                        dir = newCamPos - followPos;
                        float dv = dir.magnitude;
                        if (dv > Epsilon)
                        {
                            dir /= dv;
                            var ray = new Ray(followPos, dir);
                            if (c.Raycast(ray, out hitInfo, dv + m_CameraRadius))
                                newCamPos = ray.GetPoint(hitInfo.distance) - dir * k_PrecisionSlush;
                        }
                    }
                    if (Physics.ComputePenetration(scratchCollider, newCamPos, Quaternion.identity,
                        c, c.transform.position, c.transform.rotation, out var oDir, out var oDist))
                        newCamPos += oDir * oDist;
                }
                result = newCamPos - cameraPos;
            }

            if (distance > Epsilon && m_MinimumDistanceFromTarget > Epsilon)
            {
                float   minDist   = Mathf.Max(m_MinimumDistanceFromTarget, m_CameraRadius) + k_PrecisionSlush;
                Vector3 newOffset = cameraPos + result - followPos;
                if (newOffset.magnitude < minDist)
                    result = followPos - cameraPos + dir * minDist;
            }
            return result;
        }

        bool CheckForFollowTargetObstructions(CameraState state, Vector3 followPos)
        {
            Vector3 pos      = state.CorrectedPosition;
            Vector3 dir      = followPos - pos;
            float   distance = dir.magnitude;
            if (distance < Mathf.Max(m_MinimumDistanceFromTarget, Epsilon)) return true;
            return RaycastIgnoreTag(new Ray(pos, dir.normalized), out _,
                distance - m_MinimumDistanceFromTarget, m_CollideAgainst & ~m_TransparentLayers);
        }
    }
}
