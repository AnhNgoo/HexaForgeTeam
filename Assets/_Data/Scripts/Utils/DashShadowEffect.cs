using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashShadowEffect : LoadComponents
{
    [SerializeField] private string animationName = "Dodge";
    [SerializeField] private Material shadowMaterial; //Material dùng để tạo hiệu ứng bóng
    [SerializeField] private float shadowDuration = 0.5f; //Thời gian tồn tại của bóng
    [SerializeField] private Animator animator; //Animator của nhân vật
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderer = new SkinnedMeshRenderer[0]; //SkinnedMeshRenderer của nhân vật

    protected override void LoadComponent()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.Length == 0)
            skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void CreateShadowEffect()
    {
        if (animator == null || skinnedMeshRenderer == null || skinnedMeshRenderer.Length == 0 || shadowMaterial == null)
            return;

        // --- CỐT LÕI ---
        // TẠM TẮT ANIMATOR ĐỂ LẤY POSE HIỆN TẠI MÀ KHÔNG CAN THIỆP VÀO PLAY/CROSSFADE ---
        bool wasEnabled = animator.enabled;
        if (!wasEnabled) return; // Nếu Animator đã bị tắt (VD: do script khác), không xử lý để tránh lỗi

        // Tắt Animator -> Pose của SkinnedMeshRenderer sẽ "đóng băng" tại frame hiện tại
        animator.enabled = false;

        // Tạo một GameObject cha để dễ quản lý và destroy
        GameObject shadowParent = new GameObject("DashShadow_Instance");
        shadowParent.transform.position = transform.position;
        shadowParent.transform.rotation = transform.rotation;

        foreach (var smr in skinnedMeshRenderer)
        {
            if (smr == null || !smr.enabled) continue;

            // Bây giờ, Smr đã ở đúng pose hiện tại của animation đang chạy
            Mesh bakedMesh = new Mesh();
            smr.BakeMesh(bakedMesh);

            GameObject partObj = new GameObject(smr.name + "_ShadowPart");
            partObj.transform.parent = shadowParent.transform;

            // Copy transform của renderer gốc để giữ đúng vị trí/rotation/scale
            partObj.transform.position = smr.transform.position;
            partObj.transform.rotation = smr.transform.rotation;
            partObj.transform.localScale = smr.transform.lossyScale;

            MeshFilter mf = partObj.AddComponent<MeshFilter>();
            mf.mesh = bakedMesh;

            MeshRenderer mr = partObj.AddComponent<MeshRenderer>();
            mr.material = new Material(shadowMaterial);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        // NGAY LẬP TỨC BẬT LẠI ANIMATOR ĐỂ HOẠT ẢNH TIẾP TỤC CHẠY MƯỢT MÀ
        animator.enabled = wasEnabled;

        Destroy(shadowParent, shadowDuration);
    }
}
