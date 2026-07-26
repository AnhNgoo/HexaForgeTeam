using TMPro;
using UnityEngine;

public class UILevelText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtLevel;

    private void Awake()
    {
        if (txtLevel == null)
            txtLevel = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (LevelManager.Instance == null)
            return;

        txtLevel.text = $"Level : {LevelManager.Instance.CurrentLevel}";
    }
}