using TMPro;
using UnityEngine;

public class CharacterSelectUI :
    MonoBehaviour
{
    [SerializeField]
    private TMP_Text statusText;
    [SerializeField]
private GameObject lyraLock;

[SerializeField]
private GameObject aresLock;

[SerializeField]
private GameObject elaraLock;
private void Start()
{
    RefreshUI();
}
private void RefreshUI()
{
    lyraLock.SetActive(
        !CharacterManager.Instance
        .IsUnlocked(
            CharacterType.Lyra));

    aresLock.SetActive(
        !CharacterManager.Instance
        .IsUnlocked(
            CharacterType.Ares));

    elaraLock.SetActive(
        !CharacterManager.Instance
        .IsUnlocked(
            CharacterType.Elara));
}

    public void SelectKael()
    {
        SelectCharacter(
            CharacterType.Kael);
    }

    public void SelectLyra()
    {
        SelectCharacter(
            CharacterType.Lyra);
    }

    public void SelectAres()
    {
        SelectCharacter(
            CharacterType.Ares);
    }

    public void SelectElara()
    {
        SelectCharacter(
            CharacterType.Elara);
    }

    private void SelectCharacter(
        CharacterType type)
    {
        if (!CharacterManager.Instance
            .IsUnlocked(type))
        {
            statusText.text =
                "Character Locked";

            return;
        }

        CharacterManager.Instance
            .SelectCharacter(type);

        statusText.text =
            $"Selected: {type}";
    }
}