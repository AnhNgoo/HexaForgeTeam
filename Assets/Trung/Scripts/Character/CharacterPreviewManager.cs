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

    public void RefreshPreview()
    {
        CharacterType selected =
            CharacterManager.Instance
            .GetSelectedCharacter();

        kaelPreview.SetActive(
            selected == CharacterType.Kael);

        lyraPreview.SetActive(
            selected == CharacterType.Lyra);

        aresPreview.SetActive(
            selected == CharacterType.Ares);

        elaraPreview.SetActive(
            selected == CharacterType.Elara);
    }
}