using TMPro;
using UnityEngine;

public class UILevelCostText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtCost;

    private void Awake()
    {
        if (txtCost == null)
            txtCost = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (LevelManager.Instance == null)
            return;

        if (LevelManager.Instance.CurrentLevel >= 15)
        {
            txtCost.text = "MAX LEVEL";
            return;
        }

        txtCost.text =
            $"Cost : {LevelManager.Instance.GetCurrentLevelUpCost()}";
    }
}