using UnityEngine;
using DG.Tweening;

public class CharacterPreviewManager : MonoBehaviour
{
    [Header("Character Preview Objects")]
    [SerializeField] private GameObject kaelPreview;
    [SerializeField] private GameObject lyraPreview;
    [SerializeField] private GameObject aresPreview;
    [SerializeField] private GameObject elaraPreview;

    [Header("Rotate Root Target")]
    [Tooltip("Kéo Bệ đá hoặc GameObject cha chứa các Model vào đây để Reset góc xoay")]
    [SerializeField] private Transform previewRootTransform;

    [Header("Lighting & Visual Effects")]
    [SerializeField] private Light pedestalLight;
    [SerializeField] private Color physicalColor = new Color(1f, 0.4f, 0.1f);
    [SerializeField] private Color magicalColor = new Color(0.2f, 0.6f, 1f);

    private Quaternion initialRotation;

    private void Awake()
    {
        if (previewRootTransform == null)
        {
            previewRootTransform = transform;
        }
        initialRotation = previewRootTransform.localRotation;
    }

    private void OnEnable()
    {
        ResetPreviewRotation();
        RefreshPreview();
    }

    /// <summary>
    /// Hàm Reset góc xoay 3D về mặc định
    /// </summary>
    public void ResetPreviewRotation()
    {
        if (previewRootTransform != null)
        {
            previewRootTransform.DOKill();
            previewRootTransform.localRotation = initialRotation;
        }
    }

    public void RefreshPreview()
    {
        CharacterType deployedType = CharacterType.Kael;

        if (CharacterManager.Instance != null)
        {
            deployedType = CharacterManager.Instance.GetSelectedCharacter();
        }

        RefreshPreview(deployedType);
    }

    public void RefreshPreview(CharacterType typeToPreview)
    {
        // Mỗi lần chọn tướng mới -> Reset góc xoay trước
        ResetPreviewRotation();

        SetModelActive(kaelPreview, typeToPreview == CharacterType.Kael);
        SetModelActive(lyraPreview, typeToPreview == CharacterType.Lyra);
        SetModelActive(aresPreview, typeToPreview == CharacterType.Ares);
        SetModelActive(elaraPreview, typeToPreview == CharacterType.Elara);

        if (pedestalLight != null)
        {
            Color targetColor = (typeToPreview == CharacterType.Lyra || typeToPreview == CharacterType.Elara) 
                ? magicalColor 
                : physicalColor;

            pedestalLight.DOKill();
            pedestalLight.DOColor(targetColor, 0.4f);
        }
    }

    private void SetModelActive(GameObject modelObj, bool active)
    {
        if (modelObj == null) return;

        if (active)
        {
            modelObj.SetActive(true);

            // Hiệu ứng xuất hiện
            modelObj.transform.DOKill();
            modelObj.transform.localScale = Vector3.one * 0.8f;
            modelObj.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
        else
        {
            modelObj.SetActive(false);
        }
    }
}