using UnityEngine;

public class CharacterPreviewManager : MonoBehaviour
{
    [SerializeField] private GameObject kaelPreview;
    [SerializeField] private GameObject lyraPreview;
    [SerializeField] private GameObject aresPreview;
    [SerializeField] private GameObject elaraPreview;

    private void OnEnable()
    {
        RefreshPreview();
    }

    /// <summary>
    /// Hàm gốc không tham số - Giúp tương thích hoàn toàn với CharacterMenu.cs và script của bạn bạn
    /// </summary>
    public void RefreshPreview()
    {
        if (CharacterManager.Instance != null)
        {
            RefreshPreview(CharacterManager.Instance.GetSelectedCharacter());
        }
        else
        {
            RefreshPreview(CharacterType.Kael);
        }
    }

    /// <summary>
    /// Hàm có tham số - Dùng cho CharacterSelectUI khi người dùng bấm xem thử nhân vật
    /// </summary>
    public void RefreshPreview(CharacterType typeToPreview)
    {
        if (kaelPreview != null) kaelPreview.SetActive(typeToPreview == CharacterType.Kael);
        if (lyraPreview != null) lyraPreview.SetActive(typeToPreview == CharacterType.Lyra);
        if (aresPreview != null) aresPreview.SetActive(typeToPreview == CharacterType.Ares);
        if (elaraPreview != null) elaraPreview.SetActive(typeToPreview == CharacterType.Elara);
    }
}